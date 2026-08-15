using System;
using System.Collections.Generic;
using System.Linq;
using UtinComputerTest.Gameplay.Configs;
using UtinComputerTest.Gameplay.Level;
using UtinComputerTest.Gameplay.Views;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Runtime
{
    public sealed class GameplayLevelRuntime : IDisposable
    {
        private readonly GameplayConfig _gameplayConfig;
        private readonly GameplayAssets _gameplayAssets;
        private readonly PlayerPathService _playerPathService;
        private readonly List<ObstacleRuntime> _obstacles = new();
        private readonly Stack<ObstacleView> _obstaclePool = new();

        private DoorView _doorView;

        public GameplayLevelRuntime(
            GameplayConfig gameplayConfig,
            GameplayAssets gameplayAssets,
            PlayerPathService playerPathService)
        {
            _gameplayConfig = gameplayConfig;
            _gameplayAssets = gameplayAssets;
            _playerPathService = playerPathService;
        }

        public MapView MapView { get; private set; }
        public PlayerBallRuntime Player { get; private set; }
        public ProjectileRuntime Projectile { get; private set; }
        public DoorRuntime Door { get; private set; }
        public Vector3 DoorPosition { get; private set; }
        public IReadOnlyList<ObstacleRuntime> Obstacles => _obstacles;

        public void Initialize()
        {
            MapView = UnityEngine.Object.Instantiate(_gameplayAssets.MapPrefab);
            MapView.SetVisualYaw(_gameplayConfig.RoadVisualYaw);

            var playerView = UnityEngine.Object.Instantiate(_gameplayAssets.PlayerPrefab, MapView.transform);
            Player = new PlayerBallRuntime(playerView, _gameplayConfig);
            Projectile = new ProjectileRuntime(MapView.GeneratedContentRoot, _gameplayConfig, _gameplayAssets);
        }

        public void Build(GeneratedGridLevelLayout layout)
        {
            MapView.RoadView.SetLayout(
                _gameplayConfig.NavigationFieldSize.x,
                _gameplayConfig.NavigationFieldSize.y + _gameplayConfig.CameraVisualFieldExtension * 2f,
                _gameplayConfig.NavigationFieldSize.y * 0.5f);

            foreach (var sector in layout.Sectors)
            {
                foreach (var generatedObstacle in sector.Obstacles)
                {
                    CreateObstacle(generatedObstacle);
                }
            }

            var lastObstaclePosition = _obstacles.Max(obstacle => obstacle.Position.z);
            DoorPosition = new Vector3(0f, 0.75f, lastObstaclePosition) + _gameplayConfig.DoorSpawnOffset;
            _doorView = UnityEngine.Object.Instantiate(_gameplayAssets.DoorPrefab, MapView.transform);
            _doorView.SetPosition(DoorPosition);
            Door = new DoorRuntime(_doorView);
            Door.Reset();
            _playerPathService.RebuildObstacleCells(_obstacles);
        }

        public void RecycleDestroyedObstacle(ObstacleRuntime obstacle)
        {
            obstacle.Destroy();
            _obstacles.Remove(obstacle);
            _obstaclePool.Push(obstacle.View);
        }

        public void Clear()
        {
            if (_doorView != null)
            {
                UnityEngine.Object.Destroy(_doorView.gameObject);
                _doorView = null;
                Door = null;
            }

            foreach (var obstacle in _obstacles)
            {
                obstacle.View.gameObject.SetActive(false);
                _obstaclePool.Push(obstacle.View);
            }

            _obstacles.Clear();
        }

        public void Dispose()
        {
            Clear();
            Projectile.Dispose();
            UnityEngine.Object.Destroy(MapView.gameObject);
        }

        private void CreateObstacle(GeneratedGridObstacle generatedObstacle)
        {
            var obstacleView = GetObstacleView();
            var cellWorldSize = _playerPathService.CellWorldSize;
            var worldSize = new Vector2(
                generatedObstacle.Footprint.x * cellWorldSize.x,
                generatedObstacle.Footprint.y * cellWorldSize.y);
            var radius = Mathf.Max(worldSize.x, worldSize.y) * 0.5f;

            obstacleView.name = $"Obstacle {generatedObstacle.Footprint.x}x{generatedObstacle.Footprint.y}";
            obstacleView.SetLayer(
                generatedObstacle.BlocksPlayer
                    ? _gameplayConfig.RequiredObstacleLayer
                    : _gameplayConfig.DecorativeObstacleLayer);
            obstacleView.ResetVisual();
            obstacleView.SetPosition(
                _playerPathService.GetFootprintCenter(
                    generatedObstacle.Anchor,
                    generatedObstacle.Footprint,
                    radius));
            obstacleView.SetScale(new Vector3(worldSize.x, Mathf.Max(worldSize.x, worldSize.y), worldSize.y));

            _obstacles.Add(new ObstacleRuntime(
                obstacleView,
                radius,
                generatedObstacle.BlocksPlayer,
                generatedObstacle.IsPathTarget,
                generatedObstacle.Anchor,
                generatedObstacle.Footprint,
                _gameplayAssets.InfectedObstacleMaterial));
        }

        private ObstacleView GetObstacleView()
        {
            if (_obstaclePool.TryPop(out var obstacleView))
            {
                obstacleView.gameObject.SetActive(true);
                obstacleView.transform.SetParent(MapView.GeneratedContentRoot);
                return obstacleView;
            }

            return UnityEngine.Object.Instantiate(_gameplayAssets.ObstaclePrefab, MapView.GeneratedContentRoot);
        }
    }
}
