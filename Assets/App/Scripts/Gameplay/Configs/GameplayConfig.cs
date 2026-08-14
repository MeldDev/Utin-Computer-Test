using System.Collections.Generic;
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
        [SerializeField] private AnimationCurve _energyToPlayerScale = AnimationCurve.Linear(0f, 0.5f, 100f, 2.4f);
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
        [SerializeField] private float _playerObstacleClearance = 0.1f;
        [SerializeField] private float _roadVisualYaw = 25f;
        [SerializeField] private float _cameraFieldPadding = 2f;
        [SerializeField] private float _cameraSizeMultiplier = 1.15f;
        [SerializeField] private float _cameraDistanceToField = 30f;
        [SerializeField] private LayerMask _playerMovementObstacleMask = 1 << 8;
        [SerializeField] private int _requiredObstacleLayer = 8;
        [SerializeField] private int _decorativeObstacleLayer = 9;

        [Header("Navigation")]
        [SerializeField] private Vector2 _navigationFieldSize = new(80f, 120f);
        [SerializeField] private Vector2Int _navigationGridSize = new(80, 120);
        [SerializeField] private float _doorNavigationRadius = 2f;
        [SerializeField] private int _navigationStartClearCells = 10;
        [SerializeField] private int _navigationDoorClearCells = 10;
        [SerializeField] private int _navigationSectorLengthCells = 20;
        [SerializeField] private int _navigationJumpLengthCells = 10;
        [SerializeField] private Vector2Int _navigationObstaclesPerSector = new(4, 7);
        [SerializeField] private Vector2Int _navigationChainObstaclesPerCluster = new(3, 5);
        [SerializeField] private int _navigationPathClusterHalfWidthCells = 3;
        [SerializeField] private List<int> _navigationObstacleAreas = new() { 1, 4, 9 };
        [SerializeField] private int _navigationRandomObstacleCount = 200;
        [SerializeField] private bool _drawNavigationDebugGrid = true;
        [SerializeField] private float _navigationDebugHeight = 0.05f;

        [Header("Generation")]
        [SerializeField] private float _clusterMinDistanceFactor = 0.35f;
        [SerializeField] private float _clusterMaxDistanceFactor = 0.78f;
        [SerializeField] private float _clusterSeparationMultiplier = 1.35f;
        [SerializeField] private int _placementAttempts = 24;
        [SerializeField] private float _obstacleRadius = 0.45f;
        [SerializeField] private Vector2Int _decorativeObstaclesPerCluster = new(1, 3);
        [SerializeField] private Vector2 _decorativeObstacleOffsetRange = new(2f, 5f);

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
        public float PlayerObstacleClearance => _playerObstacleClearance;
        public float RoadVisualYaw => _roadVisualYaw;
        public float CameraFieldPadding => _cameraFieldPadding;
        public float CameraSizeMultiplier => _cameraSizeMultiplier;
        public float CameraDistanceToField => _cameraDistanceToField;
        public LayerMask PlayerMovementObstacleMask => _playerMovementObstacleMask;
        public int RequiredObstacleLayer => _requiredObstacleLayer;
        public int DecorativeObstacleLayer => _decorativeObstacleLayer;
        public Vector2 NavigationFieldSize => _navigationFieldSize;
        public Vector2Int NavigationGridSize => _navigationGridSize;
        public float DoorNavigationRadius => _doorNavigationRadius;
        public int NavigationStartClearCells => _navigationStartClearCells;
        public int NavigationDoorClearCells => _navigationDoorClearCells;
        public int NavigationSectorLengthCells => _navigationSectorLengthCells;
        public int NavigationJumpLengthCells => _navigationJumpLengthCells;
        public Vector2Int NavigationObstaclesPerSector => _navigationObstaclesPerSector;
        public Vector2Int NavigationChainObstaclesPerCluster => _navigationChainObstaclesPerCluster;
        public int NavigationPathClusterHalfWidthCells => _navigationPathClusterHalfWidthCells;
        public IReadOnlyList<int> NavigationObstacleAreas => _navigationObstacleAreas;
        public int NavigationRandomObstacleCount => _navigationRandomObstacleCount;
        public bool DrawNavigationDebugGrid => _drawNavigationDebugGrid;
        public float NavigationDebugHeight => _navigationDebugHeight;
        public float ClusterMinDistanceFactor => _clusterMinDistanceFactor;
        public float ClusterMaxDistanceFactor => _clusterMaxDistanceFactor;
        public float ClusterSeparationMultiplier => _clusterSeparationMultiplier;
        public int PlacementAttempts => _placementAttempts;
        public float ObstacleRadius => _obstacleRadius;
        public Vector2Int DecorativeObstaclesPerCluster => _decorativeObstaclesPerCluster;
        public Vector2 DecorativeObstacleOffsetRange => _decorativeObstacleOffsetRange;

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
