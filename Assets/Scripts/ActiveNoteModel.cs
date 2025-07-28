using R3;
using System;
using System.Collections.Generic;

namespace LightingMidiPiano
{
    // Data structures of note On/Off events.
    public record NoteOnInfo(int NoteNumber, float Velocity);
    public record NoteOffInfo(int NoteNumber);

    public class ActiveNoteModel
    {
        public Observable<NoteOnInfo> OnNoteOn => _onNoteOn;
        readonly Subject<NoteOnInfo> _onNoteOn = new();

        public Observable<NoteOffInfo> OnNoteOff => _onNoteOff;
        readonly Subject<NoteOffInfo> _onNoteOff = new();

        // noteNumber: int -> velocity: velocity
        public IReadOnlyDictionary<int, float> ActiveNotes => _activeNotes;
        readonly Dictionary<int, float> _activeNotes = new();

        public void NoteOn(int noteNumber, float velocity)
        {
            if (_activeNotes.ContainsKey(noteNumber)) return;

            _activeNotes[noteNumber] = velocity;
            _onNoteOn.OnNext(new NoteOnInfo(noteNumber, velocity));
        }

        public void NoteOff(int noteNumber)
        {
            if (!_activeNotes.Remove(noteNumber)) return;

            _onNoteOff.OnNext(new NoteOffInfo(noteNumber));
        }

        public void Dispose()
        {
            _onNoteOn.Dispose();
            _onNoteOff.Dispose();
        }
    }
}