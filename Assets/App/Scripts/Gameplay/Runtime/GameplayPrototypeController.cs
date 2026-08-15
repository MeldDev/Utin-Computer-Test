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
        private GameplayConfig _gameplayConfig;
        private GameplayLevelSequence _levelSequence;
        private PlayerBallRuntime _player;
        private PlayerMovementRuntime _playerMovementRuntime;
        private GameplayLevelRuntime _levelRuntime;
        private PlayerPathService _playerPathService;
        private IGameplayFlowService _gameplayFlowService;
        private ProjectileRuntime _projectile;
        private ObstacleRuntime _projectileTarget;
        private GeneratedGridLevelLayout _gridLayout;
        private GameplayDebugView _debugView;
        private GameplayCameraView _cameraView;
        private GameplayState _state;
        private int _levelIndex;
        private bool _movingToDoor;
        private bool _reachedDoor;
        private bool _reachedMovementTarget;
        private string _movementBlockDebug = "Path not checked.";

        public void Initialize(GameplayConfig gameplayConfig, GameplayLevelSequence levelSequence, GameplayAssets gameplayAssets, GameplayDebugView debugView, GameplayCameraView cameraView, PlayerPathService playerPathService, IGameplayFlowService gameplayFlowService)
        {
            _gameplayConfig = gameplayConfig;
            _levelSequence = levelSequence;
            _debugView = debugView;
            _cameraView = cameraView;
            _playerPathService = playerPathService;
            _gameplayFlowService = gameplayFlowService;
            _levelRuntime = new GameplayLevelRuntime(_gameplayConfig, gameplayAssets, _playerPathService);
            _levelRuntime.Initialize();
            _player = _levelRuntime.Player;
            _projectile = _levelRuntime.Projectile;
            _playerMovementRuntime = new PlayerMovementRuntime(_player, _gameplayConfig);
            var input = gameObject.AddComponent<GameplayInputView>();
            input.Pressed.Subscribe(_ => BeginCharge()).AddTo(_disposables);
            input.Released.Subscribe(_ => ReleaseCharge()).AddTo(_disposables);
            _gameplayFlowService.NextLevelRequested.Subscribe(_ => StartLevel(_levelIndex + 1)).AddTo(_disposables);
            _gameplayFlowService.LevelRestartRequested.Subscribe(_ => RestartLevel()).AddTo(_disposables);
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

            if (_movingToDoor && _levelRuntime.Door != null && Vector3.Distance(_player.Position, _levelRuntime.DoorPosition) <= _gameplayConfig.DoorOpenDistance)
            {
                _levelRuntime.Door.Open(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _levelRuntime.Dispose();
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
            if (_playerPathService == null || _levelRuntime == null || !_gameplayConfig.DrawNavigationDebugGrid)
            {
                return;
            }

            var gridSize = _playerPathService.GridSize;
            for (var x = 0; x < gridSize.x; x++)
            {
                for (var y = 0; y < gridSize.y; y++)
                {
                    var cell = new Vector2Int(x, y);
                    var position = _levelRuntime.MapView.transform.TransformPoint(_playerPathService.GetCellCenter(cell, _gameplayConfig.NavigationDebugHeight));
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
                var from = _levelRuntime.MapView.transform.TransformPoint(_playerPathService.GetGridCorner(new Vector2Int(x, 0), _gameplayConfig.NavigationDebugHeight));
                var to = _levelRuntime.MapView.transform.TransformPoint(_playerPathService.GetGridCorner(new Vector2Int(x, gridSize.y), _gameplayConfig.NavigationDebugHeight));
                Gizmos.DrawLine(from, to);
            }

            Gizmos.color = Color.cyan;
            for (var y = 0; y <= gridSize.y; y++)
            {
                var from = _levelRuntime.MapView.transform.TransformPoint(_playerPathService.GetGridCorner(new Vector2Int(0, y), _gameplayConfig.NavigationDebugHeight));
                var to = _levelRuntime.MapView.transform.TransformPoint(_playerPathService.GetGridCorner(new Vector2Int(gridSize.x, y), _gameplayConfig.NavigationDebugHeight));
                Gizmos.DrawLine(from, to);
            }

            Gizmos.color = Color.yellow;
            var movementCells = _playerPathService.MovementCells;
            for (var index = 1; index < movementCells.Count; index++)
            {
                var from = _levelRuntime.MapView.transform.TransformPoint(_playerPathService.GetCellCenter(movementCells[index - 1], _gameplayConfig.NavigationDebugHeight));
                var to = _levelRuntime.MapView.transform.TransformPoint(_playerPathService.GetCellCenter(movementCells[index], _gameplayConfig.NavigationDebugHeight));
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

                        var position = _levelRuntime.MapView.transform.TransformPoint(_playerPathService.GetCellCenter(new Vector2Int(x, y), _gameplayConfig.NavigationDebugHeight));
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
            _movingToDoor = false;
            _reachedDoor = false;
            ClearLevel();
            _playerPathService.SetLayout();
            _gridLayout = new GridLevelLayoutGenerator(_gameplayConfig).Generate(levelConfig);
            _player.Reset(levelConfig.InitialEnergy);
            _player.SetPosition(_playerPathService.GetCellCenter(_playerPathService.GetStartCell(_gameplayConfig.GetPlayerScale(_player.Energy) * 0.5f), 0.75f));
            _levelRuntime.Build(_gridLayout);
            _cameraView.FrameProgression(
                _levelRuntime.MapView.transform.TransformPoint(_player.Position),
                _levelRuntime.MapView.transform.TransformPoint(_levelRuntime.DoorPosition),
                _gameplayConfig.CameraPlayerViewportPosition,
                _gameplayConfig.CameraDoorViewportPosition,
                _gameplayConfig.CameraFieldPadding,
                _gameplayConfig.CameraSizeMultiplier,
                _gameplayConfig.CameraDistanceToField);
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

            _projectile.Prepare(_player.Position + Vector3.forward * _gameplayConfig.ProjectileSpawnDistance);
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
                _projectile.Deactivate();
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
                _projectile.Deactivate();
                _state = GameplayState.Idle;
                return;
            }

            var targetObstacle = LookAtClosestPathObstacle();
            if (targetObstacle != null)
            {
                _projectileTarget = targetObstacle;
                _projectile.Launch(targetObstacle.Position - _projectile.Position);
            }
            else
            {
                _projectileTarget = null;
                _projectile.Launch(Vector3.forward);
            }
            _state = GameplayState.ProjectileFlying;
        }

        private ObstacleRuntime LookAtClosestPathObstacle()
        {
            var playerRadius = _gameplayConfig.GetPlayerScale(_player.Energy) * 0.5f;
            var targetObstacle = _playerPathService.GetClosestPathObstacle(_levelRuntime.Obstacles, _player.Position, playerRadius);
            if (targetObstacle != null)
            {
                _player.LookAt(targetObstacle.Position);
            }

            return targetObstacle;
        }

        private void TickProjectile(float deltaTime)
        {
            _projectile.Tick(deltaTime);
            var hitObstacle = _projectileTarget != null
                && _projectileTarget.State == ObstacleState.Normal
                && Vector3.Distance(_projectileTarget.Position, _projectile.Position) <= _projectileTarget.Radius + _projectile.Size * 0.5f
                ? _projectileTarget
                : null;
            if (hitObstacle != null)
            {
                StartCoroutine(Infect(hitObstacle, _projectile.InfectionRadius));
                _projectile.Deactivate();
                _projectileTarget = null;
                _state = GameplayState.Infection;
                return;
            }

            if (_projectile.Position.z > _gameplayConfig.NavigationFieldSize.y + 5f)
            {
                _projectile.Deactivate();
                _projectileTarget = null;
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
                foreach (var neighbour in _levelRuntime.Obstacles.Where(obstacle => obstacle.State == ObstacleState.Normal && IsInsideInfectionRange(current, obstacle, infectionRadius)).ToArray())
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
                _levelRuntime.RecycleDestroyedObstacle(obstacle);
            }

            CheckSectorCleared();
        }

        private void CheckSectorCleared()
        {
            if (TryBuildPathToNextTarget(out var path))
            {
                _movingToDoor = _reachedDoor;
                _playerMovementRuntime.Begin(path);
                _state = GameplayState.PlayerMoving;
                return;
            }

            _state = GameplayState.Idle;
            CheckUnableToShoot();
        }

        private bool TryBuildPathToNextTarget(out IReadOnlyList<Vector3> path)
        {
            var playerRadius = _gameplayConfig.GetPlayerScale(_player.Energy) * 0.5f;
            _playerPathService.RebuildObstacleCells(_levelRuntime.Obstacles);
            var targetCell = _playerPathService.GetDoorApproachCell(playerRadius);
            if (!_playerPathService.TryBuildForwardMovement(_player.Position, targetCell, playerRadius, out path, out _reachedMovementTarget))
            {
                _movementBlockDebug = $"Obstacle directly ahead. Player stops at {_playerPathService.GetCell(_player.Position)}.";
                return false;
            }

            _reachedDoor = _reachedMovementTarget;
            var occupiedRadius = _playerPathService.GetOccupiedRadiusInCells(playerRadius);
            _movementBlockDebug = $"Straight jump: {path.Count} landing points. Player occupies {occupiedRadius.x * 2 + 1} x {occupiedRadius.y * 2 + 1} cells.";
            return true;
        }

        private void CheckUnableToShoot()
        {
            var playerRadius = _gameplayConfig.GetPlayerScale(_player.Energy) * 0.5f;
            if (_playerPathService.GetClosestPathObstacle(_levelRuntime.Obstacles, _player.Position, playerRadius) != null && _player.Energy < _gameplayConfig.MinimumShotEnergy)
            {
                Lose();
            }
        }

        private void TickPlayerMovement(float deltaTime)
        {
            if (_playerMovementRuntime.Tick(deltaTime))
            {
                if (_movingToDoor)
                {
                    Win();
                }
                else
                {
                    _state = GameplayState.Idle;
                    CheckUnableToShoot();
                }
            }
        }

        private void Lose()
        {
            if (_state == GameplayState.Lose)
            {
                return;
            }

            _state = GameplayState.Lose;
            _gameplayFlowService.ReportLevelLost();
        }

        private void Win()
        {
            _state = GameplayState.Win;
            _gameplayFlowService.ReportLevelWon();
        }

        private void ClearLevel()
        {
            _projectile.Deactivate();

            _projectileTarget = null;

            _levelRuntime.Clear();
        }

        private static bool IsInsideInfectionRange(ObstacleRuntime sourceObstacle, ObstacleRuntime targetObstacle, float infectionRadius)
        {
            var centerDistance = Vector3.Distance(sourceObstacle.Position, targetObstacle.Position);
            var edgeDistance = centerDistance - sourceObstacle.Radius - targetObstacle.Radius;
            return edgeDistance <= infectionRadius;
        }
    }
}
