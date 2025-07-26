using UnityEngine;
using UnityEngine.InputSystem;
using Minis;
using System.Linq;
using System.Collections.Generic;

namespace LightingMidiPiano
{
    public class VirtualKeyboardController : MonoBehaviour
    {
        public GameObject[] keyboardKeys;
        [SerializeField] Material keyMaterial;
        [ColorUsage(true, true)]
        [SerializeField] Color _emissionColor = new Color(187f / 255f, 106f / 255f, 79f / 255f);
        [SerializeField] float _maxIntensity = 2f;

        Renderer[] _keyRenderers;
        MaterialPropertyBlock _propertyBlock;
        List<MidiDevice> _midiDevices = new List<MidiDevice>();
        static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        const int MidiNoteA0 = 21;
        int _firstMidiNoteForMapping;

        void Start()
        {
            _propertyBlock = new();
            _keyRenderers = new Renderer[keyboardKeys.Length];

            for(var i=0;i<keyboardKeys.Length;i++)
                if (keyboardKeys[i] != null)
                {
                    _keyRenderers[i] = keyboardKeys[i].GetComponent<Renderer>();
                    _keyRenderers[i].material = keyMaterial;
                }

            _firstMidiNoteForMapping = MidiNoteA0;
        }

        void OnEnable()
        {
            FindAndSubscribeMidiDevices();
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            UnsubscribeAllMidiDevices();
        }

        private void FindAndSubscribeMidiDevices()
        {
            UnsubscribeAllMidiDevices();
            _midiDevices.Clear();

            foreach (var device in InputSystem.devices.OfType<MidiDevice>())
            {
                _midiDevices.Add(device);
                SubscribeMidiDevice(device);
            }

            if (_midiDevices.Count == 0)
            {
                Debug.LogWarning("No MIDI devices found in Input System. Is your MIDI keyboard connected and recognized? And is Minis initialized correctly?");
            }
        }

        private void SubscribeMidiDevice(MidiDevice device)
        {
            device.onWillNoteOn += OnMidiNoteOn;
            device.onWillNoteOff += OnMidiNoteOff;
            Debug.Log($"Subscribed to MIDI device: {device.displayName}");
        }

        private void UnsubscribeAllMidiDevices()
        {
            foreach (var device in _midiDevices)
            {
                if (device != null) // Check if the device has already been destroyed
                {
                    device.onWillNoteOn -= OnMidiNoteOn;
                    device.onWillNoteOff -= OnMidiNoteOff;
                }
            }
            _midiDevices.Clear();
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is MidiDevice midiDevice)
            {
                if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
                {
                    if (!_midiDevices.Contains(midiDevice))
                    {
                        _midiDevices.Add(midiDevice);
                        SubscribeMidiDevice(midiDevice);
                    }
                }
                else if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
                {
                    if (_midiDevices.Contains(midiDevice))
                    {
                        midiDevice.onWillNoteOn -= OnMidiNoteOn;
                        midiDevice.onWillNoteOff -= OnMidiNoteOff;
                        _midiDevices.Remove(midiDevice);
                        Debug.Log($"Unsubscribed from MIDI device: {midiDevice.displayName}");
                    }
                }
            }
        }

        void OnMidiNoteOn(MidiNoteControl noteControl, float velocity)
        {
            var keyIndex = MapMidiNoteToKeyIndex(noteControl.noteNumber);
            if (IsValidKeyIndex(keyIndex))
                SetKeyLight(keyIndex, true);
       }

        void OnMidiNoteOff(MidiNoteControl noteControl)
        {
            var keyIndex = MapMidiNoteToKeyIndex(noteControl.noteNumber);
            if (IsValidKeyIndex(keyIndex))
                SetKeyLight(keyIndex, false);
        }

        void SetKeyLight(int keyIndex, bool isOn, float velocity)
        {
            var keyRenderer = _keyRenderers[keyIndex];
            if (keyRenderer == null) return;

            keyRenderer.GetPropertyBlock(_propertyBlock);
            if (isOn)
            {
                var intensity = Mathf.Lerp(0.1f, _maxIntensity, velocity);
                var finalColor = _emissionColor * intensity;
                _propertyBlock.SetColor(EmissionColorID, finalColor);
            }
            else
                _propertyBlock.SetColor(EmissionColorID, Color.black);

            keyRenderer.SetPropertyBlock(_propertyBlock);
        }

        void SetKeyLight(int keyIndex, bool isOn)
        {
            var keyRenderer = _keyRenderers[keyIndex];
            if (keyRenderer == null) return;

            keyRenderer.GetPropertyBlock(_propertyBlock);
            if (isOn)
                _propertyBlock.SetColor(EmissionColorID, _emissionColor);
            else
                _propertyBlock.SetColor(EmissionColorID, Color.black);

            keyRenderer.SetPropertyBlock(_propertyBlock);
        }

        int MapMidiNoteToKeyIndex(int midiNoteNumber)
        {
            int index = midiNoteNumber - _firstMidiNoteForMapping;

            if (index >= 0 && index < keyboardKeys.Length)
            {
                return index;
            }
            return -1;
        }

        bool IsValidKeyIndex(int index)
            => index >= 0 && index < _keyRenderers.Length;
    }
}
