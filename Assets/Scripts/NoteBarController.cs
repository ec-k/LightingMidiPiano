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
        Vector3 _direction = Vector3.back;

        LineRenderer _lineRenderer;

        void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        void Update()
        {
            switch (_currentState)
            {
                case State.Growing:
                    {
                        var growthAmount = _growthSpeed * Time.deltaTime;
                        var dPosition = _direction * growthAmount;
                        var endPoint = _lineRenderer.GetPosition(1);
                        _lineRenderer.SetPosition(1, endPoint + dPosition);
                    }
                    break;
                case State.Scrolling:
                    {
                        var dPosition = _direction * _scrollSpeed * Time.deltaTime;
                        var startPoint = _lineRenderer.GetPosition(0);
                        var endPoint = _lineRenderer.GetPosition(1);
                        _lineRenderer.SetPosition(0, startPoint + dPosition);
                        _lineRenderer.SetPosition(1, endPoint + dPosition);
                        if (Mathf.Abs(startPoint.z) > _objectBreakDistance)
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