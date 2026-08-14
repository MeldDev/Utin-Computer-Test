using UnityEngine;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class GameplayCameraView : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        public void FrameProgression(Vector3 playerPosition, Vector3 doorPosition, Vector2 playerViewportPosition, Vector2 doorViewportPosition, float padding, float sizeMultiplier, float distanceToField)
        {
            var cameraTransform = _camera.transform;
            var progression = doorPosition - playerPosition;
            var viewportDelta = doorViewportPosition - playerViewportPosition;
            var horizontalSize = Mathf.Abs(Vector3.Dot(progression, cameraTransform.right)) / (2f * _camera.aspect * Mathf.Abs(viewportDelta.x));
            var verticalSize = Mathf.Abs(Vector3.Dot(progression, cameraTransform.up)) / (2f * Mathf.Abs(viewportDelta.y));
            _camera.orthographicSize = (Mathf.Max(horizontalSize, verticalSize) + padding) * sizeMultiplier;

            var progressionCenter = (playerPosition + doorPosition) * 0.5f;
            cameraTransform.position = progressionCenter - cameraTransform.forward * distanceToField;
            var desiredPlayerRightOffset = (playerViewportPosition.x - 0.5f) * _camera.orthographicSize * _camera.aspect * 2f;
            var desiredPlayerUpOffset = (playerViewportPosition.y - 0.5f) * _camera.orthographicSize * 2f;
            var playerOffset = playerPosition - cameraTransform.position;
            cameraTransform.position += cameraTransform.right * (Vector3.Dot(playerOffset, cameraTransform.right) - desiredPlayerRightOffset);
            cameraTransform.position += cameraTransform.up * (Vector3.Dot(playerOffset, cameraTransform.up) - desiredPlayerUpOffset);
        }
    }
}
