using UnityEngine;
using UtinComputerTest.Gameplay.Views;

namespace UtinComputerTest.Gameplay.Configs
{
    [CreateAssetMenu(menuName = "Utin Computer Test/Gameplay/Gameplay Config", fileName = "GameplayConfig")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [Header("Prefabs")]
        [SerializeField] private MapView _mapPrefab;
        [SerializeField] private ObstacleView _obstaclePrefab;

        [Header("Energy")]
        [SerializeField] private float _maxEnergy = 100f;
        [SerializeField] private float _minimumPlayerEnergy = 5f;
        [SerializeField] private float _minimumShotEnergy = 4f;
        [SerializeField] private float _maximumShotEnergy = 30f;
        [SerializeField] private float _chargeSpeed = 16f;

        [Header("Energy Curves")]
        [SerializeField] private AnimationCurve _energyToPlayerScale = AnimationCurve.Linear(0f, 0.35f, 100f, 1.4f);
        [SerializeField] private AnimationCurve _energyToProjectileScale = AnimationCurve.Linear(0f, 0.2f, 30f, 1.1f);
        [SerializeField] private AnimationCurve _energyToInfectionRadius = AnimationCurve.Linear(0f, 0.6f, 30f, 3.5f);

        [Header("Projectile and infection")]
        [SerializeField] private float _projectileSpeed = 18f;
        [SerializeField] private float _infectionStepDelay = 0.08f;
        [SerializeField] private float _explosionDelay = 0.22f;

        [Header("Player and door")]
        [SerializeField] private float _playerMoveSpeed = 8f;
        [SerializeField] private float _playerJumpHeight = 0.45f;
        [SerializeField] private float _doorOpenDistance = 5f;
        [SerializeField] private float _playerStopDistanceBeforeSector = 2.5f;

        [Header("Generation")]
        [SerializeField] private float _clusterMinDistanceFactor = 0.35f;
        [SerializeField] private float _clusterMaxDistanceFactor = 0.78f;
        [SerializeField] private float _clusterSeparationMultiplier = 1.35f;
        [SerializeField] private int _placementAttempts = 24;
        [SerializeField] private float _obstacleRadius = 0.45f;

        public float MaxEnergy => _maxEnergy;
        public MapView MapPrefab => _mapPrefab;
        public ObstacleView ObstaclePrefab => _obstaclePrefab;
        public float MinimumPlayerEnergy => _minimumPlayerEnergy;
        public float MinimumShotEnergy => _minimumShotEnergy;
        public float MaximumShotEnergy => _maximumShotEnergy;
        public float ChargeSpeed => _chargeSpeed;
        public float ProjectileSpeed => _projectileSpeed;
        public float InfectionStepDelay => _infectionStepDelay;
        public float ExplosionDelay => _explosionDelay;
        public float PlayerMoveSpeed => _playerMoveSpeed;
        public float PlayerJumpHeight => _playerJumpHeight;
        public float DoorOpenDistance => _doorOpenDistance;
        public float PlayerStopDistanceBeforeSector => _playerStopDistanceBeforeSector;
        public float ClusterMinDistanceFactor => _clusterMinDistanceFactor;
        public float ClusterMaxDistanceFactor => _clusterMaxDistanceFactor;
        public float ClusterSeparationMultiplier => _clusterSeparationMultiplier;
        public int PlacementAttempts => _placementAttempts;
        public float ObstacleRadius => _obstacleRadius;

        public float GetPlayerScale(float energy)
        {
            return _energyToPlayerScale.Evaluate(energy);
        }

        public float GetProjectileScale(float energy)
        {
            return _energyToProjectileScale.Evaluate(energy);
        }

        public float GetInfectionRadius(float energy)
        {
            return _energyToInfectionRadius.Evaluate(energy);
        }
    }
}
