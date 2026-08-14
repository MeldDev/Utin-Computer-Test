using UnityEngine;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class GameplayCameraView : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        public void FrameField(Transform mapTransform, float roadWidth, float roadLength, float padding, float sizeMultiplier, float distanceToField)
        {
            var cameraTransform = _camera.transform;
            var center = mapTransform.TransformPoint(new Vector3(0f, 0f, roadLength * 0.5f));
            cameraTransform.position = center - cameraTransform.forward * distanceToField;
            var corners = new[]
            {
                mapTransform.TransformPoint(new Vector3(-roadWidth * 0.5f, 0f, 0f)),
                mapTransform.TransformPoint(new Vector3(roadWidth * 0.5f, 0f, 0f)),
                mapTransform.TransformPoint(new Vector3(-roadWidth * 0.5f, 0f, roadLength)),
                mapTransform.TransformPoint(new Vector3(roadWidth * 0.5f, 0f, roadLength))
            };
            var verticalExtent = 0f;
            var horizontalExtent = 0f;
            foreach (var corner in corners)
            {
                var offset = corner - center;
                verticalExtent = Mathf.Max(verticalExtent, Mathf.Abs(Vector3.Dot(offset, cameraTransform.up)));
                horizontalExtent = Mathf.Max(horizontalExtent, Mathf.Abs(Vector3.Dot(offset, cameraTransform.right)));
            }

            _camera.orthographicSize = (Mathf.Max(verticalExtent, horizontalExtent / _camera.aspect) + padding) * sizeMultiplier;
        }
    }
}
