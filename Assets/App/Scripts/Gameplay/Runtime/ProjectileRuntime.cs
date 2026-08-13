using UtinComputerTest.Gameplay.Configs;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Runtime
{
    public sealed class ProjectileRuntime
    {
        private readonly GameObject _gameObject;
        private readonly GameplayConfig _config;

        public ProjectileRuntime(GameObject gameObject, GameplayConfig config)
        {
            _gameObject = gameObject;
            _config = config;
        }

        public float Energy { get; private set; }
        public float Size { get; private set; }
        public float InfectionRadius { get; private set; }
        public Vector3 Direction { get; private set; }
        public Vector3 Position => _gameObject.transform.position;

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
        }

        public void Tick(float deltaTime)
        {
            _gameObject.transform.position += Direction * _config.ProjectileSpeed * deltaTime;
        }

        public void Destroy()
        {
            Object.Destroy(_gameObject);
        }
    }
}
