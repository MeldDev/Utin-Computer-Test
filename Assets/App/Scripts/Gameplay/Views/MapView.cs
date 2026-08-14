using UnityEngine;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class MapView : MonoBehaviour
    {
        [SerializeField] private RoadView _roadView;
        [SerializeField] private Transform _generatedContentRoot;

        public RoadView RoadView => _roadView;
        public Transform GeneratedContentRoot => _generatedContentRoot;

        public void SetVisualYaw(float visualYaw)
        {
            transform.localRotation = Quaternion.Euler(0f, visualYaw, 0f);
        }

    }
}
