using UtinComputerTest.Gameplay.Configs;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Runtime
{
    public sealed class ProjectileRuntime
    {
        private readonly GameObject _gameObject;
        private readonly Renderer _renderer;
        private readonly GameplayConfig _config;

        public ProjectileRuntime(Transform parent, GameplayConfig config, GameplayAssets gameplayAssets)
        {
            _gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _gameObject.name = "Projectile";
            _gameObject.transform.SetParent(parent);
            _renderer = _gameObject.GetComponent<Renderer>();
            _config = config;
            _renderer.sharedMaterial = gameplayAssets.ProjectileMaterial;
            _gameObject.SetActive(false);
        }

        public float Energy { get; private set; }
        public float Size { get; private set; }
        public float InfectionRadius { get; private set; }
        public Vector3 Direction { get; private set; }
        public Vector3 Position => _gameObject.transform.localPosition;

        public void Prepare(Vector3 position)
        {
            _gameObject.transform.localPosition = position;
            _gameObject.transform.localRotation = Quaternion.identity;
            _gameObject.SetActive(true);
            SetEnergy(0f);
        }

        public void SetEnergy(float energy)
        {
            Energy = energy;
            Size = _config.GetProjectileScale(energy);
            InfectionRadius = _config.GetInfectionRadius(energy);
            _gameObject.transform.localScale = Vector3.one * Size;
        }

        public void Launch(Vector3 direction)
        {
            Direction = direction.normalized;
            _gameObject.transform.localRotation = Quaternion.LookRotation(Direction);
        }

        public void Tick(float deltaTime)
        {
            _gameObject.transform.localPosition += Direction * _config.ProjectileSpeed * deltaTime;
        }

        public void Deactivate()
        {
            _gameObject.SetActive(false);
        }

        public void Dispose()
        {
            Object.Destroy(_gameObject);
        }
    }
}
