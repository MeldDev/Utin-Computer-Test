using UnityEngine;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class ObstacleView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        private Material _defaultMaterial;

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

        public void ResetVisual()
        {
            _defaultMaterial ??= _renderer.sharedMaterial;
            _renderer.sharedMaterial = _defaultMaterial;
        }

        public void PlayInfection(Material infectedMaterial)
        {
            _renderer.sharedMaterial = infectedMaterial;
            transform.localScale *= 1.15f;
        }
    }
}
