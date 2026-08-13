using UnityEngine;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class DoorView : MonoBehaviour
    {
        [SerializeField] private Transform _leftPanel;
        [SerializeField] private Transform _rightPanel;
        private Vector3 _leftClosedPosition;
        private Vector3 _rightClosedPosition;

        private void Awake()
        {
            _leftClosedPosition = _leftPanel.localPosition;
            _rightClosedPosition = _rightPanel.localPosition;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void ResetDoor()
        {
            _leftPanel.localPosition = _leftClosedPosition;
            _rightPanel.localPosition = _rightClosedPosition;
        }

        public void Open(float deltaTime)
        {
            _leftPanel.localPosition = Vector3.MoveTowards(_leftPanel.localPosition, _leftClosedPosition + Vector3.left * 2f, deltaTime * 3f);
            _rightPanel.localPosition = Vector3.MoveTowards(_rightPanel.localPosition, _rightClosedPosition + Vector3.right * 2f, deltaTime * 3f);
        }
    }
}
