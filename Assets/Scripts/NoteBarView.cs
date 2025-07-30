using System.Collections.Generic;
using UnityEngine;

namespace LightingMidiPiano
{
    public class NoteBarView : MonoBehaviour
    {
        [SerializeField] NoteBarController _whiteNoteBarPrefab;
        [SerializeField] NoteBarController _blackNoteBarPrefab;
        [SerializeField] float _zPositionOffset = -0.058726904f;

        readonly Dictionary<int, NoteBarController> _activeNoteBars = new();

        /// <summary>
        /// Create a note bar at a position of specifed key.
        /// </summary>
        /// <param name="noteNumber">a MIDI note number for management.</param>
        /// <param name="velocity"></param>
        /// <param name="keyTransform">a transform of specifed keys</param>
        public void CreateNoteBar(int noteNumber, float velocity, Transform keyTransform)
        {
            if (_activeNoteBars.ContainsKey(noteNumber)) return;

            var keyPosition = keyTransform.position;
            var spawnPosition = new Vector3(keyPosition.x, keyPosition.y, _zPositionOffset);
            var notePrefab = IsBlackKey(noteNumber) ? _blackNoteBarPrefab : _whiteNoteBarPrefab;
            var noteBarInstance = Instantiate(notePrefab, spawnPosition, Quaternion.identity);

            // TODO: ベロシティに応じて色や太さを変えるならここ

            _activeNoteBars.Add(noteNumber, noteBarInstance);
        }

        public void TriggerNoteOff(int noteNumber)
        {
            if (_activeNoteBars.TryGetValue(noteNumber, out var noteBar))
            {
                noteBar.NoteOff();
                _activeNoteBars.Remove(noteNumber);
            }
        }

        bool IsBlackKey(int midiNoteNumber)
        {
            return (midiNoteNumber % 12) switch
            {
                1 or 3 or 6 or 8 or 10 => true,
                _ => false,
            };
        }
    }
}