using UnityEngine;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class PlayerBallView : MonoBehaviour
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
    }
}
