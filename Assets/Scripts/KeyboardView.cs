using UnityEngine;

namespace LightingMidiPiano
{
    public class KeyboardView : MonoBehaviour
    {
        public GameObject[] KeyboardKeys;
        [SerializeField] Color _pressedBaseColor = new(0f, 0.5550244f, 0.7490196f);
        [ColorUsage(true, true)]
        [SerializeField] Color _pressedEmissionColor = new(0.03626161f, 0.4568826f, 0.69886f);
        [SerializeField] float _maxIntensity = 2f;
        [SerializeField] Color _defaultWhiteKeyColor = Color.white;
        [SerializeField] Color _defaultBlackKeyColor = Color.black;

        Renderer[] _keyRenderers;
        MaterialPropertyBlock _propertyBlock;

        const int MidiNoteA0 = 21;
        static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
        static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

        void Start()
        {
            _propertyBlock = new();
            _keyRenderers = new Renderer[KeyboardKeys.Length];
            for (var i = 0; i < KeyboardKeys.Length; i++)
                if (KeyboardKeys[i] != null)
                    _keyRenderers[i] = KeyboardKeys[i].GetComponent<Renderer>();
        }

        /// <summary>
        /// Returns a key transform corresponding to specified MIDI note number
        /// </summary>
        /// <param name="noteNumber">MIDI note number</param>
        /// <returns>A transform of corresponding key. When it is not found, this function returns null.</returns>
        public Transform GetKeyTransform(int noteNumber)
        {
            var keyIndex = MapMidiNoteToKeyIndex(noteNumber);
            if (!IsValidKeyIndex(keyIndex)) return null;

            return KeyboardKeys[keyIndex]?.transform;
        }

        public void SetKeyOn(int noteNumber)
        {
            var keyIndex = MapMidiNoteToKeyIndex(noteNumber);
            if (!IsValidKeyIndex(keyIndex)) return;

            var keyRenderer = _keyRenderers[keyIndex];
            keyRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(EmissionColorID, _pressedEmissionColor);
            _propertyBlock.SetColor(BaseColorID, _pressedBaseColor);

            keyRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetKeyOn(int noteNumber, float velocity)
        {
            var keyIndex = MapMidiNoteToKeyIndex(noteNumber);
            if (!IsValidKeyIndex(keyIndex)) return;

            var keyRenderer = _keyRenderers[keyIndex];
            keyRenderer.GetPropertyBlock(_propertyBlock);

            var intensity = Mathf.Lerp(0.1f, _maxIntensity, velocity);
            var finalColor = _pressedEmissionColor * intensity;
            _propertyBlock.SetColor(EmissionColorID, finalColor);
            _propertyBlock.SetColor(BaseColorID, _pressedBaseColor);

            keyRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetKeyOff(int noteNumber)
        {
            var keyIndex = MapMidiNoteToKeyIndex(noteNumber);
            if (!IsValidKeyIndex(keyIndex)) return;

            var keyRenderer = _keyRenderers[keyIndex];
            keyRenderer.GetPropertyBlock(_propertyBlock);

            var isBlack = IsBlackKey(noteNumber);
            var baseColor = isBlack ? _defaultBlackKeyColor : _defaultWhiteKeyColor;
            _propertyBlock.SetColor(BaseColorID, baseColor);
            _propertyBlock.SetColor(EmissionColorID, Color.black);

            keyRenderer.SetPropertyBlock(_propertyBlock);
        }

        int MapMidiNoteToKeyIndex(int midiNoteNumber) => midiNoteNumber - MidiNoteA0;
        bool IsValidKeyIndex(int index) => index >= 0 && index < _keyRenderers.Length;
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