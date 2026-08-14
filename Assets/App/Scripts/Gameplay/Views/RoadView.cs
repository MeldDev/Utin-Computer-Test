using UnityEngine;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class RoadView : MonoBehaviour
    {
        public void SetLayout(float width, float length)
        {
            transform.localPosition = new Vector3(0f, -0.15f, length * 0.5f);
            transform.localScale = new Vector3(width, 0.3f, length);
        }
    }
}
