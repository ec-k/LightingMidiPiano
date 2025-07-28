using Minis;
using R3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace LightingMidiPiano
{
    public class MidiInputPresenter : IStartable, IDisposable
    {
        readonly ActiveNoteModel _model;
        readonly KeyboardView _keyboardView;
        readonly CompositeDisposable _disposables = new();

        readonly List<MidiDevice> _subscribedDevices = new();

        public MidiInputPresenter(ActiveNoteModel model, KeyboardView keyboardView)
        {
            _model = model;
            _keyboardView = keyboardView;
        }

        void IStartable.Start()
        {
            _model.OnNoteOn
                .Subscribe(info =>
                {
                    _keyboardView.SetKeyOn(info.NoteNumber, info.Velocity);
                })
                .AddTo(_disposables);

            _model.OnNoteOff
                .Subscribe(info =>
                {
                    _keyboardView.SetKeyOff(info.NoteNumber);
                })
                .AddTo(_disposables);

            foreach (var device in InputSystem.devices.OfType<MidiDevice>())
                SubscribeToDevice(device);

            InputSystem.onDeviceChange += OnDeviceChange;
        }

        void IDisposable.Dispose()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            foreach (var device in _subscribedDevices)
                UnsubscribeFromDevice(device);
            _subscribedDevices.Clear();

            _disposables.Dispose();
            _model.Dispose();
        }

        void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is not MidiDevice midiDevice) return;

            switch (change)
            {
                case InputDeviceChange.Added:
                    UnityEngine.Debug.Log($"MIDI Device Added: {midiDevice.displayName}");
                    SubscribeToDevice(midiDevice);
                    break;

                case InputDeviceChange.Removed:
                    UnityEngine.Debug.Log($"MIDI Device Removed: {midiDevice.displayName}");
                    UnsubscribeFromDevice(midiDevice);
                    break;
            }
        }

        void SubscribeToDevice(MidiDevice device)
        {
            if (_subscribedDevices.Contains(device)) return;

            device.onWillNoteOn += OnMidiNoteOn;
            device.onWillNoteOff += OnMidiNoteOff;
            _subscribedDevices.Add(device);
            UnityEngine.Debug.Log($"Subscribed to: {device.displayName}");
        }

        void UnsubscribeFromDevice(MidiDevice device)
        {
            if (!_subscribedDevices.Contains(device)) return;

            if (device != null)
            {
                device.onWillNoteOn -= OnMidiNoteOn;
                device.onWillNoteOff -= OnMidiNoteOff;
            }
            _subscribedDevices.Remove(device);
            UnityEngine.Debug.Log($"Unsubscribed from: {device.displayName}");
        }

        void OnMidiNoteOn(MidiNoteControl note, float velocity)
        {
            _model.NoteOn(note.noteNumber, velocity);
        }

        void OnMidiNoteOff(MidiNoteControl note)
        {
            _model.NoteOff(note.noteNumber);
        }
    }
}