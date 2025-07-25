using UnityEngine;
using UnityEngine.InputSystem;
using Minis;
using System.Linq;
using System.Collections.Generic;

public class VirtualKeyboardController : MonoBehaviour
{
    // Array of virtual keyboard GameObjects
    // Needs to consider the offset between array index and MIDI note number
    // E.g., keyboardKeys[0] corresponds to MIDI note 60 (C4)
    public GameObject[] keyboardKeys;

    // List of currently active MidiDevices
    private List<MidiDevice> _midiDevices = new List<MidiDevice>();

    void OnEnable()
    {
        // Get and subscribe to all existing MIDI devices
        FindAndSubscribeMidiDevices();

        // Register event to subscribe when a new device is connected
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        // Unsubscribe from events when the application quits or the script is disabled
        InputSystem.onDeviceChange -= OnDeviceChange;
        UnsubscribeAllMidiDevices();
    }

    private void FindAndSubscribeMidiDevices()
    {
        // Unsubscribe from existing subscriptions (to avoid duplicates when re-running)
        UnsubscribeAllMidiDevices();
        _midiDevices.Clear(); // Also clear the list

        // Get all MidiDevices, add them to the list, and subscribe
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
        // Subscribe to onWillNoteOn and onWillNoteOff events
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

    // InputSystem device change event handler
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is MidiDevice midiDevice)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
            {
                // If a new MidiDevice is added, subscribe to it and add to the list
                if (!_midiDevices.Contains(midiDevice))
                {
                    _midiDevices.Add(midiDevice);
                    SubscribeMidiDevice(midiDevice);
                }
            }
            else if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
            {
                // If a device is disconnected, unsubscribe and remove from the list
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

    // MIDI Note On event handler
    void OnMidiNoteOn(MidiNoteControl noteControl, float velocity)
    {
        // Get the note number from MidiNoteControl
        int noteNumber = noteControl.noteNumber; // MidiNoteControl should have a noteNumber property

        Debug.Log($"Note On: {noteNumber}, Velocity: {velocity}");

        // Convert MIDI note number to virtual keyboard index
        int keyIndex = MapMidiNoteToKeyIndex(noteNumber);

        if (keyIndex != -1 && keyIndex < keyboardKeys.Length && keyboardKeys[keyIndex] != null)
        {
            // Logic to light up the key
            SetKeyLight(keyboardKeys[keyIndex], true, velocity);
        }
    }

    // MIDI Note Off event handler
    void OnMidiNoteOff(MidiNoteControl noteControl)
    {
        // Get the note number from MidiNoteControl
        int noteNumber = noteControl.noteNumber;

        Debug.Log($"Note Off: {noteNumber}");

        // Convert MIDI note number to virtual keyboard index
        int keyIndex = MapMidiNoteToKeyIndex(noteNumber);

        if (keyIndex != -1 && keyIndex < keyboardKeys.Length && keyboardKeys[keyIndex] != null)
        {
            // Logic to revert the key to its original state
            SetKeyLight(keyboardKeys[keyIndex], false);
        }
    }

    // Helper function for lighting up/turning off keys
    private void SetKeyLight(GameObject keyObject, bool isOn, float velocity = 0)
    {
        Renderer keyRenderer = keyObject.GetComponent<Renderer>();
        if (keyRenderer != null)
        {
            Material material = keyRenderer.material; // Modify the instantiated material directly

            if (isOn)
            {
                // Example: Adjust emission intensity based on velocity
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.yellow * (velocity + 0.1f)); // 0.1f is minimum emission
                // material.color = Color.Lerp(Color.gray, Color.yellow, velocity); // To change the color itself
            }
            else
            {
                // Example: Turn off emission and revert color
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black); // Turn off emission completely
                // material.color = Color.white; // To revert to normal color
            }
        }
    }

    // Logic to map MIDI note number to virtual keyboard array index
    // Adjust this to match your virtual keyboard's layout
    private int MapMidiNoteToKeyIndex(int midiNoteNumber)
    {
        // Example: Assuming the virtual keyboard starts from MIDI note 60 (C4) for an 88-key piano
        // Here, index 0 of the keyboardKeys array corresponds to MIDI note 60.
        const int firstMidiNote = 60; // MIDI note number of the lowest note on the virtual keyboard (e.g., C4)
        int index = midiNoteNumber - firstMidiNote;

        if (index >= 0 && index < keyboardKeys.Length)
        {
            return index;
        }
        return -1; // No corresponding key found
    }
}