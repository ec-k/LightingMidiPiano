using System.Collections.Generic;
using UnityEngine;

namespace LightingMidiPiano
{
    public class NoteBarView : MonoBehaviour
    {
        [SerializeField] NoteBarController _noteBarPrefab;
        [SerializeField] Vector3 _positionOffset;

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

            Vector3 spawnPosition = keyTransform.position + _positionOffset;
            var noteBarInstance = Instantiate(_noteBarPrefab, spawnPosition, Quaternion.identity);

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
    }
}