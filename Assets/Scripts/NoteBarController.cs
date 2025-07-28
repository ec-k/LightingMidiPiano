using UnityEngine;

namespace LightingMidiPiano
{
    public class NoteBarController : MonoBehaviour
    {
        [SerializeField] Transform _meshTransform;
        [SerializeField] float _growthSpeed = 0.5f;
        [SerializeField] float _scrollSpeed = 0.5f;

        enum State { Growing, Scrolling }
        State _currentState = State.Growing;

        float _objectBreakDistance = 1f;

        void Update()
        {
            switch (_currentState)
            {
                case State.Growing:
                    float growthAmount = _growthSpeed * Time.deltaTime;
                    _meshTransform.localScale += new Vector3(0, 0, growthAmount);
                    _meshTransform.localPosition += new Vector3(0, 0, - growthAmount / 2.0f);
                    break;
                case State.Scrolling:
                    transform.position += Vector3.back * _scrollSpeed * Time.deltaTime;
                    if (transform.position.y > _objectBreakDistance)
                    {
                        Destroy(gameObject);
                    }
                    break;
            }
        }

        public void NoteOff()
        {
            _currentState = State.Scrolling;
        }
    }
}