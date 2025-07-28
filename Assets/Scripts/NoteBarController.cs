using UnityEngine;

namespace LightingMidiPiano
{
    public class NoteBarController : MonoBehaviour
    {
        [SerializeField] float _growthSpeed = 0.1f;
        [SerializeField] float _scrollSpeed = 0.1f;

        enum State { Growing, Scrolling }
        State _currentState = State.Growing;

        float _objectBreakDistance = 1f;

        void Update()
        {
            switch (_currentState)
            {
                case State.Growing:
                    float growthAmount = _growthSpeed * Time.deltaTime;
                    transform.localScale += new Vector3(0, 0, growthAmount);
                    transform.localPosition += new Vector3(0, 0, - growthAmount / 2.0f);
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