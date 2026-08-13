using UnityEngine;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class ObstacleView : MonoBehaviour
    {
        public Vector3 Position => transform.position;

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }

        public void PlayInfection()
        {
            transform.localScale *= 1.15f;
        }
    }
}
