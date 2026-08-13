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
        private GameplayConfig _gameplayConfig;
        private LevelSequence _levelSequence;
        private PlayerBallRuntime _player;
        private ProjectileRuntime _projectile;
        private DoorRuntime _door;
        private GeneratedLevelLayout _layout;
        private Transform _levelRoot;
        private MapView _mapView;
        private GameplayState _state;
        private int _levelIndex;
        private int _sectorIndex;
        private bool _movingToDoor;
        private Vector3 _movementTarget;
        private float _movementStartY;

        public void Initialize(GameplayConfig gameplayConfig, LevelSequence levelSequence)
        {
            _gameplayConfig = gameplayConfig;
            _levelSequence = levelSequence;
            _mapView = Instantiate(_gameplayConfig.MapPrefab);
            _levelRoot = _mapView.GeneratedContentRoot;
            _player = new PlayerBallRuntime(_mapView.PlayerBallView, _gameplayConfig);
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

            if (_movingToDoor && _door != null && Vector3.Distance(_player.Position, _layout.DoorPosition) <= _gameplayConfig.DoorOpenDistance)
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

            if (_mapView.DebugView != null)
            {
                _mapView.DebugView.SetText($"Energy: {_player.Energy:0.0} / {_gameplayConfig.MaxEnergy:0}\nLevel {_levelSequence.GetLevel(_levelIndex)?.LevelId} | {_state}\nHold mouse/touch to charge. Release to shoot. R = restart.");
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
            ClearLevel();
            _layout = new LevelLayoutGenerator(_gameplayConfig).Generate(levelConfig);
            var validation = new LevelValidator(_gameplayConfig).Validate(levelConfig, _layout);
            Debug.Log($"Level {levelConfig.LevelId}: expected usage {validation.ExpectedUsage:0.##}, reserve {validation.Reserve:0.##}, valid: {validation.IsValid}.");
            foreach (var warning in validation.Warnings)
            {
                Debug.LogWarning($"Level {levelConfig.LevelId}: {warning}");
            }
            BuildLevel(levelConfig, _layout);
            _player.SetPosition(new Vector3(0f, 0.75f, 0f));
            _player.Reset(levelConfig.InitialEnergy);
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
            projectileObject.transform.position = _player.Position + Vector3.forward * 1.1f;
            _projectile = new ProjectileRuntime(projectileObject, _gameplayConfig);
            _projectile.SetEnergy(0f);
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

            _projectile.Launch(Vector3.forward);
            _state = GameplayState.ProjectileFlying;
        }

        private void TickProjectile(float deltaTime)
        {
            _projectile.Tick(deltaTime);
            var hitObstacle = _obstacles.FirstOrDefault(obstacle => obstacle.State == ObstacleState.Normal && Vector3.Distance(obstacle.Position, _projectile.Position) <= obstacle.Radius + _projectile.Size * 0.5f);
            if (hitObstacle != null)
            {
                StartCoroutine(Infect(hitObstacle, _projectile.InfectionRadius));
                _projectile.Destroy();
                _projectile = null;
                _state = GameplayState.Infection;
                return;
            }

            if (_projectile.Position.z > _layout.DoorPosition.z + 5f)
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
                foreach (var neighbour in _obstacles.Where(obstacle => obstacle.State == ObstacleState.Normal && Vector3.Distance(obstacle.Position, current.Position) <= infectionRadius).ToArray())
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
            }

            CheckSectorCleared();
        }

        private void CheckSectorCleared()
        {
            if (_sectorIndex >= _sectorObstacles.Count || CanPlayerPassCurrentSector())
            {
                _sectorIndex++;
                _movingToDoor = _sectorIndex >= _layout.Sectors.Count;
                _movementTarget = _movingToDoor ? _layout.DoorPosition + Vector3.forward * 1.5f : new Vector3(0f, 0.75f, _layout.Sectors[_sectorIndex].StopPositionZ);
                _movementStartY = _player.Position.y;
                _state = GameplayState.PlayerMoving;
                return;
            }

            _state = GameplayState.Idle;
            CheckUnableToShoot();
        }

        private bool CanPlayerPassCurrentSector()
        {
            var playerRadius = _gameplayConfig.GetPlayerScale(_player.Energy) * 0.5f;
            var targetZ = _sectorIndex + 1 < _layout.Sectors.Count
                ? _layout.Sectors[_sectorIndex + 1].StopPositionZ
                : _layout.DoorPosition.z;

            foreach (var obstacle in _sectorObstacles[_sectorIndex])
            {
                if (obstacle.State == ObstacleState.Destroyed)
                {
                    continue;
                }

                var isAlongPath = obstacle.Position.z >= _player.Position.z && obstacle.Position.z <= targetZ;
                var blocksCentralCorridor = Mathf.Abs(obstacle.Position.x) <= playerRadius + obstacle.Radius;
                if (isAlongPath && blocksCentralCorridor)
                {
                    return false;
                }
            }

            return true;
        }

        private void CheckUnableToShoot()
        {
            if (_obstacles.Count > 0 && _player.Energy < _gameplayConfig.MinimumShotEnergy)
            {
                Lose();
            }
        }

        private void TickPlayerMovement(float deltaTime)
        {
            var current = _player.Position;
            var target = _movementTarget;
            var next = Vector3.MoveTowards(current, target, _gameplayConfig.PlayerMoveSpeed * deltaTime);
            var distance = Vector3.Distance(current, target);
            next.y = _movementStartY + Mathf.Abs(Mathf.Sin(Time.time * 10f)) * _gameplayConfig.PlayerJumpHeight;
            _player.SetPosition(next);

            if (distance <= 0.08f)
            {
                _player.SetPosition(new Vector3(target.x, _movementStartY, target.z));
                if (_movingToDoor)
                {
                    _state = GameplayState.Win;
                    StartLevel(_levelIndex + 1);
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
            _state = GameplayState.Lose;
        }

        private void BuildLevel(LevelConfig levelConfig, GeneratedLevelLayout layout)
        {
            _mapView.RoadView.SetLayout(levelConfig.RoadWidth, layout.RoadLength);

            foreach (var sector in layout.Sectors)
            {
                var sectorObstacles = new List<ObstacleRuntime>();
                foreach (var cluster in sector.Clusters)
                {
                    foreach (var position in cluster.ObstaclePositions)
                    {
                        var obstacleView = Instantiate(_gameplayConfig.ObstaclePrefab, _levelRoot);
                        obstacleView.name = $"Obstacle E{cluster.ExpectedEnergy:0.0}";
                        obstacleView.SetPosition(position);
                        obstacleView.SetScale(_gameplayConfig.ObstacleRadius * 2f);
                        var obstacle = new ObstacleRuntime(obstacleView, _gameplayConfig.ObstacleRadius);
                        _obstacles.Add(obstacle);
                        sectorObstacles.Add(obstacle);
                    }
                }

                _sectorObstacles.Add(sectorObstacles);
            }

            _mapView.DoorView.SetPosition(layout.DoorPosition);
            _door = new DoorRuntime(_mapView.DoorView);
            _door.Reset();
        }

        private void ClearLevel()
        {
            if (_projectile != null)
            {
                _projectile.Destroy();
                _projectile = null;
            }

            foreach (Transform child in _levelRoot)
            {
                Destroy(child.gameObject);
            }

            _obstacles.Clear();
            _sectorObstacles.Clear();
        }
    }
}
