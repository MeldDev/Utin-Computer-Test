using UnityEngine;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class PlayerBallView : MonoBehaviour
    {
        public Vector3 Position => transform.localPosition;

        public void SetPosition(Vector3 position)
        {
            transform.localPosition = position;
        }

        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }

        public void LookAt(Vector3 targetPosition)
        {
            var direction = targetPosition - transform.localPosition;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0f)
            {
                transform.localRotation = Quaternion.LookRotation(direction);
            }
        }
    }
}
