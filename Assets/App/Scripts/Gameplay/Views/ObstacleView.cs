using UnityEngine;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class ObstacleView : MonoBehaviour
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

        public void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        public void SetLayer(int layer)
        {
            gameObject.layer = layer;
        }

        public void PlayInfection()
        {
            transform.localScale *= 1.15f;
        }
    }
}
