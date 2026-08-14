using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UtinComputerTest.Gameplay.Configs;
using UtinComputerTest.Gameplay.Level;
using UtinComputerTest.Gameplay.Views;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Runtime
{
    public enum GameplayState
    {
        Idle,
        Charging,
        ProjectileFlying,
        Infection,
        PlayerMoving,
        Win,
        Lose
    }

    public sealed class GameplayPrototypeController : MonoBehaviour
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly List<ObstacleRuntime> _obstacles = new();
        private readonly List<List<ObstacleRuntime>> _sectorObstacles = new();
        private readonly Stack<ObstacleView> _obstaclePool = new();
        private GameplayConfig _gameplayConfig;
        private LevelSequence _levelSequence;
        private PlayerBallRuntime _player;
        private PlayerPathService _playerPathService;
        private ProjectileRuntime _projectile;
        private DoorRuntime _door;
        private GeneratedGridLevelLayout _gridLayout;
        private Transform _levelRoot;
        private MapView _mapView;
        private GameplayDebugView _debugView;
        private GameplayCameraView _cameraView;
        private GameplayState _state;
        private int _levelIndex;
        private int _sectorIndex;
        private bool _movingToDoor;
        private bool _reachedDoor;
        private bool _reachedMovementTarget;
        private readonly List<Vector3> _movementPath = new();
        private Vector3 _doorPosition;
        private float _movementStartY;
        private int _movementPathIndex;
        private string _movementBlockDebug = "Path not checked.";

        public void Initialize(GameplayConfig gameplayConfig, LevelSequence levelSequence, GameplayDebugView debugView, GameplayCameraView cameraView, PlayerPathService playerPathService)
        {
            _gameplayConfig = gameplayConfig;
            _levelSequence = levelSequence;
            _debugView = debugView;
            _cameraView = cameraView;
            _mapView = Instantiate(_gameplayConfig.MapPrefab);
            _mapView.SetVisualYaw(_gameplayConfig.RoadVisualYaw);
            _levelRoot = _mapView.GeneratedContentRoot;
            _player = new PlayerBallRuntime(_mapView.PlayerBallView, _gameplayConfig);
            _playerPathService = playerPathService;
            var input = gameObject.AddComponent<GameplayInputView>();
            input.Pressed.Subscribe(_ => BeginCharge()).AddTo(_disposables);
            input.Released.Subscribe(_ => ReleaseCharge()).AddTo(_disposables);
            input.RestartRequested.Subscribe(_ => RestartLevel()).AddTo(_disposables);
            StartLevel(0);
        }

        private void Update()
        {
            if (_state == GameplayState.Charging)
            {
                Charge(Time.deltaTime);
            }
            else if (_state == GameplayState.ProjectileFlying)
            {
                TickProjectile(Time.deltaTime);
            }
            else if (_state == GameplayState.PlayerMoving)
            {
                TickPlayerMovement(Time.deltaTime);
            }

            if (_movingToDoor && _door != null && Vector3.Distance(_player.Position, _doorPosition) <= _gameplayConfig.DoorOpenDistance)
            {
                _door.Open(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void OnGUI()
        {
            if (_player == null)
            {
                return;
            }

            if (_debugView != null)
            {
                _debugView.SetText($"Energy: {_player.Energy:0.0} / {_gameplayConfig.MaxEnergy:0}\nLevel {_levelSequence.GetLevel(_levelIndex)?.LevelId} | {_state}\n{_movementBlockDebug}\nHold mouse/touch to charge. Release to shoot. R = restart.");
            }
        }

        private void OnDrawGizmos()
        {
            if (_playerPathService == null || _mapView == null || !_gameplayConfig.DrawNavigationDebugGrid)
            {
                return;
            }

            var gridSize = _playerPathService.GridSize;
            for (var x = 0; x < gridSize.x; x++)
            {
                for (var y = 0; y < gridSize.y; y++)
                {
                    var cell = new Vector2Int(x, y);
                    var position = _mapView.transform.TransformPoint(_playerPathService.GetCellCenter(cell, _gameplayConfig.NavigationDebugHeight));
                    if (_playerPathService.IsObstacleCell(cell))
                    {
                        Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.45f);
                        Gizmos.DrawCube(position, Vector3.one * 0.04f);
                    }
                    else if (_playerPathService.IsDoorCell(cell))
                    {
                        Gizmos.color = new Color(0.8f, 0.25f, 1f, 0.5f);
                        Gizmos.DrawCube(position, Vector3.one * 0.04f);
                    }
                }
            }

            Gizmos.color = Color.cyan;
            for (var x = 0; x <= gridSize.x; x++)
            {
                var from = _mapView.transform.TransformPoint(_playerPathService.GetGridCorner(new Vector2Int(x, 0), _gameplayConfig.NavigationDebugHeight));
                var to = _mapView.transform.TransformPoint(_playerPathService.GetGridCorner(new Vector2Int(x, gridSize.y), _gameplayConfig.NavigationDebugHeight));
                Gizmos.DrawLine(from, to);
            }

            Gizmos.color = Color.cyan;
            for (var y = 0; y <= gridSize.y; y++)
            {
                var from = _mapView.transform.TransformPoint(_playerPathService.GetGridCorner(new Vector2Int(0, y), _gameplayConfig.NavigationDebugHeight));
                var to = _mapView.transform.TransformPoint(_playerPathService.GetGridCorner(new Vector2Int(gridSize.x, y), _gameplayConfig.NavigationDebugHeight));
                Gizmos.DrawLine(from, to);
            }

            Gizmos.color = Color.yellow;
            var movementCells = _playerPathService.MovementCells;
            for (var index = 1; index < movementCells.Count; index++)
            {
                var from = _mapView.transform.TransformPoint(_playerPathService.GetCellCenter(movementCells[index - 1], _gameplayConfig.NavigationDebugHeight));
                var to = _mapView.transform.TransformPoint(_playerPathService.GetCellCenter(movementCells[index], _gameplayConfig.NavigationDebugHeight));
                Gizmos.DrawLine(from, to);
            }

            if (_player != null)
            {
                var playerCell = _playerPathService.GetCell(_player.Position);
                var playerRadiusInCells = _playerPathService.GetOccupiedRadiusInCells(_gameplayConfig.GetPlayerScale(_player.Energy) * 0.5f);
                Gizmos.color = new Color(0.15f, 1f, 0.2f, 0.7f);
                for (var x = playerCell.x - playerRadiusInCells.x; x <= playerCell.x + playerRadiusInCells.x; x++)
                {
                    for (var y = playerCell.y - playerRadiusInCells.y; y <= playerCell.y + playerRadiusInCells.y; y++)
                    {
                        if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
                        {
                            continue;
                        }

                        var position = _mapView.transform.TransformPoint(_playerPathService.GetCellCenter(new Vector2Int(x, y), _gameplayConfig.NavigationDebugHeight));
                        Gizmos.DrawCube(position, Vector3.one * 0.08f);
                    }
                }
            }
        }

        private void StartLevel(int levelIndex)
        {
            var levelConfig = _levelSequence.GetLevel(levelIndex);
            if (levelConfig == null)
            {
                _state = GameplayState.Win;
                return;
            }

            _levelIndex = levelIndex;
            _sectorIndex = 0;
            _movingToDoor = false;
            _reachedDoor = false;
            ClearLevel();
            _playerPathService.SetLayout();
            _gridLayout = new GridLevelLayoutGenerator(_gameplayConfig).Generate(levelConfig);
            _player.Reset(levelConfig.InitialEnergy);
            _player.SetPosition(_playerPathService.GetCellCenter(_playerPathService.GetStartCell(_gameplayConfig.GetPlayerScale(_player.Energy) * 0.5f), 0.75f));
            BuildLevel(_gridLayout);
            _cameraView.FrameField(_mapView.transform, _gameplayConfig.NavigationFieldSize.x, _gameplayConfig.NavigationFieldSize.y, _gameplayConfig.CameraFieldPadding, _gameplayConfig.CameraSizeMultiplier, _gameplayConfig.CameraDistanceToField);
            _state = GameplayState.Idle;
        }

        private void RestartLevel()
        {
            if (_state == GameplayState.Win)
            {
                return;
            }

            StartLevel(_levelIndex);
        }

        private void BeginCharge()
        {
            if (_state != GameplayState.Idle)
            {
                return;
            }

            if (_player.Energy < _gameplayConfig.MinimumShotEnergy)
            {
                Lose();
                return;
            }

            var projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Projectile";
            projectileObject.transform.SetParent(_levelRoot);
            projectileObject.transform.localPosition = _player.Position + Vector3.forward * 1.1f;
            _projectile = new ProjectileRuntime(projectileObject, _gameplayConfig);
            _projectile.SetEnergy(0f);
            LookAtClosestPathObstacle();
            _state = GameplayState.Charging;
        }

        private void Charge(float deltaTime)
        {
            var remainingShotCapacity = _gameplayConfig.MaximumShotEnergy - _projectile.Energy;
            var transferredEnergy = _player.TransferEnergy(Mathf.Min(_gameplayConfig.ChargeSpeed * deltaTime, remainingShotCapacity));
            _projectile.SetEnergy(_projectile.Energy + transferredEnergy);

            if (_player.Energy <= _gameplayConfig.MinimumPlayerEnergy)
            {
                _projectile.Destroy();
                _projectile = null;
                Lose();
            }
        }

        private void ReleaseCharge()
        {
            if (_state != GameplayState.Charging)
            {
                return;
            }

            if (_projectile.Energy < _gameplayConfig.MinimumShotEnergy)
            {
                _player.Reset(_player.Energy + _projectile.Energy);
                _projectile.Destroy();
                _projectile = null;
                _state = GameplayState.Idle;
                return;
            }

            var targetObstacle = LookAtClosestPathObstacle();
            if (targetObstacle != null)
            {
                _projectile.Launch(targetObstacle.Position - _projectile.Position);
            }
            else
            {
                _projectile.Launch(Vector3.forward);
            }
            _state = GameplayState.ProjectileFlying;
        }

        private ObstacleRuntime LookAtClosestPathObstacle()
        {
            var targetObstacle = _playerPathService.GetClosestPathObstacle(_obstacles, _player.Position);
            if (targetObstacle != null)
            {
                _player.LookAt(targetObstacle.Position);
            }

            return targetObstacle;
        }

        private void TickProjectile(float deltaTime)
        {
            _projectile.Tick(deltaTime);
            var hitObstacle = _obstacles.FirstOrDefault(obstacle => obstacle.IsPathTarget && obstacle.State == ObstacleState.Normal && Vector3.Distance(obstacle.Position, _projectile.Position) <= obstacle.Radius + _projectile.Size * 0.5f);
            if (hitObstacle != null)
            {
                StartCoroutine(Infect(hitObstacle, _projectile.InfectionRadius));
                _projectile.Destroy();
                _projectile = null;
                _state = GameplayState.Infection;
                return;
            }

            if (_projectile.Position.z > _gameplayConfig.NavigationFieldSize.y + 5f)
            {
                _projectile.Destroy();
                _projectile = null;
                _state = GameplayState.Idle;
                CheckUnableToShoot();
            }
        }

        private IEnumerator Infect(ObstacleRuntime firstObstacle, float infectionRadius)
        {
            var infected = new List<ObstacleRuntime>();
            var frontier = new Queue<ObstacleRuntime>();
            if (firstObstacle.Infect())
            {
                infected.Add(firstObstacle);
                frontier.Enqueue(firstObstacle);
            }

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                foreach (var neighbour in _obstacles.Where(obstacle => obstacle.State == ObstacleState.Normal && IsInsideInfectionRange(current, obstacle, infectionRadius)).ToArray())
                {
                    if (neighbour.Infect())
                    {
                        infected.Add(neighbour);
                        frontier.Enqueue(neighbour);
                        yield return new WaitForSeconds(_gameplayConfig.InfectionStepDelay);
                    }
                }
            }

            yield return new WaitForSeconds(_gameplayConfig.ExplosionDelay);
            foreach (var obstacle in infected)
            {
                obstacle.Destroy();
                _obstacles.Remove(obstacle);
                _obstaclePool.Push(obstacle.View);
            }

            CheckSectorCleared();
        }

        private void CheckSectorCleared()
        {
            if (TryBuildPathToNextTarget())
            {
                _movingToDoor = _reachedDoor;
                _movementStartY = _player.Position.y;
                _movementPathIndex = 0;
                _state = GameplayState.PlayerMoving;
                return;
            }

            _state = GameplayState.Idle;
            CheckUnableToShoot();
        }

        private bool TryBuildPathToNextTarget()
        {
            var playerRadius = _gameplayConfig.GetPlayerScale(_player.Energy) * 0.5f;
            _playerPathService.RebuildObstacleCells(_obstacles);
            var targetCell = _playerPathService.GetDoorApproachCell(playerRadius);
            if (!_playerPathService.TryBuildForwardMovement(_player.Position, targetCell, playerRadius, out var path, out _reachedMovementTarget))
            {
                _movementBlockDebug = $"Obstacle directly ahead. Player stops at {_playerPathService.GetCell(_player.Position)}.";
                return false;
            }

            _movementPath.Clear();
            _movementPath.AddRange(path);
            _reachedDoor = _reachedMovementTarget;
            var occupiedRadius = _playerPathService.GetOccupiedRadiusInCells(playerRadius);
            _movementBlockDebug = $"Straight jump: {_movementPath.Count} landing points. Player occupies {occupiedRadius.x * 2 + 1} x {occupiedRadius.y * 2 + 1} cells.";
            return true;
        }

        private void CheckUnableToShoot()
        {
            if (_obstacles.Any(obstacle => obstacle.IsPathTarget) && _player.Energy < _gameplayConfig.MinimumShotEnergy)
            {
                Lose();
            }
        }

        private void TickPlayerMovement(float deltaTime)
        {
            var current = _player.Position;
            var target = _movementPath[_movementPathIndex];
            var next = Vector3.MoveTowards(current, target, _gameplayConfig.PlayerMoveSpeed * deltaTime);
            var distance = Vector3.Distance(current, target);
            next.y = _movementStartY + Mathf.Abs(Mathf.Sin(Time.time * 10f)) * _gameplayConfig.PlayerJumpHeight;
            _player.SetPosition(next);

            if (distance <= 0.08f)
            {
                _player.SetPosition(new Vector3(target.x, _movementStartY, target.z));
                _movementPathIndex++;
                if (_movementPathIndex < _movementPath.Count)
                {
                    return;
                }

                if (_movingToDoor)
                {
                    _state = GameplayState.Win;
                    StartLevel(_levelIndex + 1);
                }
                else
                {
                    if (_reachedMovementTarget)
                    {
                        _sectorIndex++;
                    }

                    _state = GameplayState.Idle;
                    CheckUnableToShoot();
                }
            }
        }

        private void Lose()
        {
            _state = GameplayState.Lose;
        }

        private void BuildLevel(GeneratedGridLevelLayout layout)
        {
            _mapView.RoadView.SetLayout(_gameplayConfig.NavigationFieldSize.x, _gameplayConfig.NavigationFieldSize.y);

            foreach (var sector in layout.Sectors)
            {
                var sectorObstacles = new List<ObstacleRuntime>();
                foreach (var generatedObstacle in sector.Obstacles)
                {
                    var obstacleView = GetObstacleView();
                    var cellWorldSize = _playerPathService.CellWorldSize;
                    var worldSize = new Vector2(generatedObstacle.Footprint.x * cellWorldSize.x, generatedObstacle.Footprint.y * cellWorldSize.y);
                    obstacleView.name = $"Obstacle {generatedObstacle.Footprint.x}x{generatedObstacle.Footprint.y}";
                    obstacleView.SetLayer(generatedObstacle.BlocksPlayer ? _gameplayConfig.RequiredObstacleLayer : _gameplayConfig.DecorativeObstacleLayer);
                    obstacleView.SetPosition(_playerPathService.GetFootprintCenter(generatedObstacle.Anchor, generatedObstacle.Footprint, Mathf.Max(worldSize.x, worldSize.y) * 0.5f));
                    obstacleView.SetScale(new Vector3(worldSize.x, Mathf.Max(worldSize.x, worldSize.y), worldSize.y));
                    var obstacle = new ObstacleRuntime(obstacleView, Mathf.Max(worldSize.x, worldSize.y) * 0.5f, generatedObstacle.BlocksPlayer, generatedObstacle.IsPathTarget, generatedObstacle.Anchor, generatedObstacle.Footprint);
                    _obstacles.Add(obstacle);
                    sectorObstacles.Add(obstacle);
                }

                _sectorObstacles.Add(sectorObstacles);
            }

            _doorPosition = _playerPathService.GetCellCenter(_playerPathService.GetDoorAnchorCell(), 0.75f);
            _mapView.DoorView.SetPosition(_doorPosition);
            _door = new DoorRuntime(_mapView.DoorView);
            _door.Reset();
            _playerPathService.RebuildObstacleCells(_obstacles);
        }

        private void ClearLevel()
        {
            if (_projectile != null)
            {
                _projectile.Destroy();
                _projectile = null;
            }

            foreach (var obstacle in _obstacles)
            {
                obstacle.View.gameObject.SetActive(false);
                _obstaclePool.Push(obstacle.View);
            }

            _obstacles.Clear();
            _sectorObstacles.Clear();
        }

        private ObstacleView GetObstacleView()
        {
            if (_obstaclePool.TryPop(out var obstacleView))
            {
                obstacleView.gameObject.SetActive(true);
                obstacleView.transform.SetParent(_levelRoot);
                return obstacleView;
            }

            return Instantiate(_gameplayConfig.ObstaclePrefab, _levelRoot);
        }

        private void SpawnDecorativeObstacles(GeneratedCluster cluster, float roadWidth, float playerRadius)
        {
            var random = new System.Random(Mathf.RoundToInt(cluster.ExpectedEnergy * 1000f) + _levelIndex);
            var count = random.Next(_gameplayConfig.DecorativeObstaclesPerCluster.x, _gameplayConfig.DecorativeObstaclesPerCluster.y + 1);
            for (var index = 0; index < count; index++)
            {
                var angle = (float)random.NextDouble() * Mathf.PI * 2f;
                var distance = Mathf.Lerp(_gameplayConfig.DecorativeObstacleOffsetRange.x, _gameplayConfig.DecorativeObstacleOffsetRange.y, (float)random.NextDouble());
                var position = cluster.ObstaclePositions[random.Next(cluster.ObstaclePositions.Count)] + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
                var minimumSideOffset = playerRadius + _gameplayConfig.ObstacleRadius + _gameplayConfig.PlayerObstacleClearance;
                var side = position.x >= 0f ? 1f : -1f;
                position.x = side * Mathf.Clamp(Mathf.Abs(position.x), minimumSideOffset, roadWidth * 0.5f - _gameplayConfig.ObstacleRadius);
                var obstacleView = GetObstacleView();
                obstacleView.name = "Decorative Obstacle";
                obstacleView.SetLayer(_gameplayConfig.DecorativeObstacleLayer);
                obstacleView.SetPosition(position);
                obstacleView.SetScale(_gameplayConfig.ObstacleRadius * 1.5f);
                _obstacles.Add(new ObstacleRuntime(obstacleView, _gameplayConfig.ObstacleRadius * 0.75f, false, false, Vector2Int.zero, Vector2Int.one));
            }
        }

        private static bool IsInsideInfectionRange(ObstacleRuntime sourceObstacle, ObstacleRuntime targetObstacle, float infectionRadius)
        {
            var centerDistance = Vector3.Distance(sourceObstacle.Position, targetObstacle.Position);
            var edgeDistance = centerDistance - sourceObstacle.Radius - targetObstacle.Radius;
            return edgeDistance <= infectionRadius;
        }
    }
}
