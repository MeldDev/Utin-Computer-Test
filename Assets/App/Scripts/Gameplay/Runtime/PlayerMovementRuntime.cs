using System;
using System.Collections.Generic;
using UtinComputerTest.Gameplay.Configs;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Runtime
{
    public sealed class PlayerMovementRuntime
    {
        private readonly PlayerBallRuntime _player;
        private readonly GameplayConfig _gameplayConfig;

        private IReadOnlyList<Vector3> _path;
        private float _startHeight;
        private int _pathIndex;

        public PlayerMovementRuntime(PlayerBallRuntime player, GameplayConfig gameplayConfig)
        {
            _player = player;
            _gameplayConfig = gameplayConfig;
        }

        public void Begin(IReadOnlyList<Vector3> path)
        {
            if (path.Count == 0)
            {
                throw new ArgumentException("Movement path must contain at least one point.", nameof(path));
            }

            _path = path;
            _startHeight = _player.Position.y;
            _pathIndex = 0;
        }

        public bool Tick(float deltaTime)
        {
            var currentPosition = _player.Position;
            var targetPosition = _path[_pathIndex];
            var nextPosition = Vector3.MoveTowards(
                currentPosition,
                targetPosition,
                _gameplayConfig.PlayerMoveSpeed * deltaTime);
            var distanceToTarget = Vector3.Distance(currentPosition, targetPosition);

            nextPosition.y = _startHeight
                + Mathf.Abs(Mathf.Sin(Time.time * 10f)) * _gameplayConfig.PlayerJumpHeight;
            _player.SetPosition(nextPosition);

            if (distanceToTarget > 0.08f)
            {
                return false;
            }

            _player.SetPosition(new Vector3(targetPosition.x, _startHeight, targetPosition.z));
            _pathIndex++;
            return _pathIndex == _path.Count;
        }
    }
}
