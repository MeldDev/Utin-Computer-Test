using UnityEngine;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class MapView : MonoBehaviour
    {
        [SerializeField] private RoadView _roadView;
        [SerializeField] private PlayerBallView _playerBallView;
        [SerializeField] private DoorView _doorView;
        [SerializeField] private GameplayDebugView _debugView;
        [SerializeField] private Transform _generatedContentRoot;

        public RoadView RoadView => _roadView;
        public PlayerBallView PlayerBallView => _playerBallView;
        public DoorView DoorView => _doorView;
        public GameplayDebugView DebugView => _debugView;
        public Transform GeneratedContentRoot => _generatedContentRoot;

        public void SetVisualYaw(float visualYaw)
        {
            transform.localRotation = Quaternion.Euler(0f, visualYaw, 0f);
        }
    }
}
