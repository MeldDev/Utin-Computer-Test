using System;
using System.Collections.Generic;
using System.Linq;
using UtinComputerTest.Gameplay.Configs;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Runtime
{
    public sealed class PlayerPathService
    {
        private readonly GameplayConfig _gameplayConfig;
        private readonly List<Vector2Int> _movementCells = new();
        private bool[,] _doorCells;
        private bool[,] _obstacleCells;
        private Vector2Int _gridSize;
        private Vector2 _fieldSize;

        public PlayerPathService(GameplayConfig gameplayConfig)
        {
            _gameplayConfig = gameplayConfig;
        }

        public Vector2Int GridSize => _gridSize;
        public Vector2 CellWorldSize => GetCellSize();
        public IReadOnlyList<Vector2Int> MovementCells => _movementCells;

        public void SetLayout()
        {
            _gridSize = _gameplayConfig.NavigationGridSize;
            if (_gridSize.x < 3 || _gridSize.y < 3)
            {
                throw new InvalidOperationException("Navigation grid must contain at least 3 cells on each axis.");
            }

            if (_gameplayConfig.NavigationJumpLengthCells < 1)
            {
                throw new InvalidOperationException("Navigation jump length must be at least one cell.");
            }

            _fieldSize = _gameplayConfig.NavigationFieldSize;
            _obstacleCells = new bool[_gridSize.x, _gridSize.y];
            _doorCells = new bool[_gridSize.x, _gridSize.y];
            MarkDoorCells();
            _movementCells.Clear();
        }

        public Vector2Int GetStartCell(float playerRadius)
        {
            return new Vector2Int(_gridSize.x / 2, GetOccupiedRadiusInCells(playerRadius).y);
        }

        public Vector2Int GetDoorApproachCell(float playerRadius)
        {
            var playerRadiusInCells = GetOccupiedRadiusInCells(playerRadius);
            var doorRadiusInCells = GetOccupiedRadiusInCells(_gameplayConfig.DoorNavigationRadius);
            return new Vector2Int(_gridSize.x / 2, _gridSize.y - 1 - doorRadiusInCells.y - playerRadiusInCells.y);
        }

        public Vector2Int GetDoorAnchorCell()
        {
            return new Vector2Int(_gridSize.x / 2, _gridSize.y - 1);
        }

        public Vector3 GetCellCenter(Vector2Int cell, float height)
        {
            var cellSize = GetCellSize();
            return new Vector3(-_fieldSize.x * 0.5f + (cell.x + 0.5f) * cellSize.x, height, (cell.y + 0.5f) * cellSize.y);
        }

        public Vector3 GetGridCorner(Vector2Int corner, float height)
        {
            var cellSize = GetCellSize();
            return new Vector3(-_fieldSize.x * 0.5f + corner.x * cellSize.x, height, corner.y * cellSize.y);
        }

        public Vector3 GetFootprintCenter(Vector2Int anchor, Vector2Int footprint, float height)
        {
            var cellSize = GetCellSize();
            return new Vector3(
                -_fieldSize.x * 0.5f + (anchor.x + footprint.x * 0.5f) * cellSize.x,
                height,
                (anchor.y + footprint.y * 0.5f) * cellSize.y);
        }

        public Vector2Int GetCell(Vector3 localPosition)
        {
            var cellSize = GetCellSize();
            return new Vector2Int(
                Mathf.Clamp(Mathf.FloorToInt((localPosition.x + _fieldSize.x * 0.5f) / cellSize.x), 0, _gridSize.x - 1),
                Mathf.Clamp(Mathf.FloorToInt(localPosition.z / cellSize.y), 0, _gridSize.y - 1));
        }

        public void RebuildObstacleCells(IReadOnlyList<ObstacleRuntime> obstacles)
        {
            Array.Clear(_obstacleCells, 0, _obstacleCells.Length);
            foreach (var obstacle in obstacles)
            {
                if (!obstacle.BlocksPlayer || obstacle.State != ObstacleState.Normal)
                {
                    continue;
                }

                for (var x = obstacle.GridAnchor.x; x < obstacle.GridAnchor.x + obstacle.GridFootprint.x; x++)
                {
                    for (var y = obstacle.GridAnchor.y; y < obstacle.GridAnchor.y + obstacle.GridFootprint.y; y++)
                    {
                        _obstacleCells[x, y] = true;
                    }
                }
            }
        }

        public bool TryBuildForwardMovement(Vector3 startPosition, Vector2Int targetCell, float playerRadius, out IReadOnlyList<Vector3> movementPath, out bool reachedTarget)
        {
            var start = GetCell(startPosition);
            _movementCells.Clear();
            reachedTarget = false;
            if (!CanOccupy(start, playerRadius))
            {
                movementPath = Array.Empty<Vector3>();
                return false;
            }

            var firstObstacleY = -1;
            for (var y = start.y + 1; y <= targetCell.y; y++)
            {
                var next = new Vector2Int(start.x, y);
                if (!CanOccupy(next, playerRadius))
                {
                    firstObstacleY = y;
                    break;
                }
            }

            var landingY = firstObstacleY < 0
                ? targetCell.y
                : Mathf.Max(start.y, firstObstacleY - _gameplayConfig.NavigationJumpLengthCells);
            AddLandingCell(new Vector2Int(start.x, landingY));
            reachedTarget = landingY == targetCell.y;
            movementPath = CreateMovementPath(startPosition.y);
            return _movementCells.Count > 0;
        }

        public ObstacleRuntime GetClosestPathObstacle(IReadOnlyList<ObstacleRuntime> obstacles, Vector3 playerPosition, float playerRadius)
        {
            var playerCell = GetCell(playerPosition);
            var occupiedRadius = GetOccupiedRadiusInCells(playerRadius);
            return obstacles
                .Where(obstacle => obstacle.BlocksPlayer
                    && obstacle.State == ObstacleState.Normal
                    && obstacle.GridAnchor.y + obstacle.GridFootprint.y > playerCell.y
                    && obstacle.GridAnchor.x <= playerCell.x + occupiedRadius.x
                    && obstacle.GridAnchor.x + obstacle.GridFootprint.x - 1 >= playerCell.x - occupiedRadius.x)
                .OrderBy(obstacle => Vector3.SqrMagnitude(obstacle.Position - playerPosition))
                .FirstOrDefault();
        }

        public bool IsObstacleCell(Vector2Int cell)
        {
            return _obstacleCells[cell.x, cell.y];
        }

        public bool IsDoorCell(Vector2Int cell)
        {
            return _doorCells[cell.x, cell.y];
        }

        public Vector2Int GetOccupiedRadiusInCells(float radius)
        {
            var cellSize = GetCellSize();
            return new Vector2Int(Mathf.CeilToInt(radius / cellSize.x), Mathf.CeilToInt(radius / cellSize.y));
        }

        private bool CanOccupy(Vector2Int center, float radius)
        {
            var occupiedRadius = GetOccupiedRadiusInCells(radius);
            for (var x = center.x - occupiedRadius.x; x <= center.x + occupiedRadius.x; x++)
            {
                for (var y = center.y - occupiedRadius.y; y <= center.y + occupiedRadius.y; y++)
                {
                    if (x < 0 || x >= _gridSize.x || y < 0 || y >= _gridSize.y || _obstacleCells[x, y])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void MarkDoorCells()
        {
            var center = GetDoorAnchorCell();
            var occupiedRadius = GetOccupiedRadiusInCells(_gameplayConfig.DoorNavigationRadius);
            for (var x = center.x - occupiedRadius.x; x <= center.x + occupiedRadius.x; x++)
            {
                for (var y = center.y - occupiedRadius.y; y <= center.y + occupiedRadius.y; y++)
                {
                    if (x >= 0 && x < _gridSize.x && y >= 0 && y < _gridSize.y)
                    {
                        _doorCells[x, y] = true;
                    }
                }
            }
        }

        private IReadOnlyList<Vector3> CreateMovementPath(float height)
        {
            var result = new List<Vector3>(_movementCells.Count);
            foreach (var cell in _movementCells)
            {
                result.Add(GetCellCenter(cell, height));
            }

            return result;
        }

        private void AddLandingCell(Vector2Int cell)
        {
            if (_movementCells.Count == 0 || _movementCells[_movementCells.Count - 1] != cell)
            {
                _movementCells.Add(cell);
            }
        }

        private Vector2 GetCellSize()
        {
            return new Vector2(_fieldSize.x / _gridSize.x, _fieldSize.y / _gridSize.y);
        }

    }
}
