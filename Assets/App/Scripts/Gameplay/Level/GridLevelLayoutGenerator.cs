using System;
using System.Collections.Generic;
using UtinComputerTest.Gameplay.Configs;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Level
{
    public sealed class GridLevelLayoutGenerator
    {
        private readonly GameplayConfig _gameplayConfig;

        public GridLevelLayoutGenerator(GameplayConfig gameplayConfig)
        {
            _gameplayConfig = gameplayConfig;
        }

        public GeneratedGridLevelLayout Generate(LevelConfig levelConfig)
        {
            var gridSize = _gameplayConfig.NavigationGridSize;
            var playableCells = gridSize.y - _gameplayConfig.NavigationStartClearCells - _gameplayConfig.NavigationDoorClearCells;
            if (playableCells <= 0 || playableCells % _gameplayConfig.NavigationSectorLengthCells != 0)
            {
                throw new InvalidOperationException("Navigation field must have an integer number of sectors after start and door clear cells are excluded.");
            }

            var random = new System.Random(levelConfig.Seed);
            var occupiedCells = new bool[gridSize.x, gridSize.y];
            var sectors = new List<GeneratedGridSector>();
            var sectorCount = playableCells / _gameplayConfig.NavigationSectorLengthCells;
            for (var sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
            {
                var startY = _gameplayConfig.NavigationStartClearCells + sectorIndex * _gameplayConfig.NavigationSectorLengthCells;
                var obstacles = new List<GeneratedGridObstacle>();
                CreatePathCluster(random, gridSize, startY, occupiedCells, obstacles);

                sectors.Add(new GeneratedGridSector(sectorIndex, startY, _gameplayConfig.NavigationSectorLengthCells, obstacles));
            }

            PlaceRandomObstacles(random, gridSize, occupiedCells, sectors);

            return new GeneratedGridLevelLayout(sectors);
        }

        private void CreatePathCluster(System.Random random, Vector2Int gridSize, int sectorStartY, bool[,] occupiedCells, List<GeneratedGridObstacle> obstacles)
        {
            if (!TryPlacePathObstacle(random, gridSize, sectorStartY, occupiedCells, obstacles, out var pathObstacle))
            {
                throw new InvalidOperationException("Unable to place a navigation obstacle on the player path.");
            }

            var clusterObstacleCount = random.Next(_gameplayConfig.NavigationChainObstaclesPerCluster.x, _gameplayConfig.NavigationChainObstaclesPerCluster.y + 1);
            for (var obstacleIndex = 1; obstacleIndex < clusterObstacleCount; obstacleIndex++)
            {
                TryPlaceClusterObstacle(random, gridSize, sectorStartY, occupiedCells, obstacles, pathObstacle);
            }
        }

        private bool TryPlacePathObstacle(System.Random random, Vector2Int gridSize, int sectorStartY, bool[,] occupiedCells, List<GeneratedGridObstacle> obstacles, out GeneratedGridObstacle pathObstacle)
        {
            var playerPathX = gridSize.x / 2;
            for (var attempt = 0; attempt < _gameplayConfig.PlacementAttempts; attempt++)
            {
                var footprint = GetFootprint(random, true);
                var anchorX = Mathf.Clamp(playerPathX - footprint.x / 2, 0, gridSize.x - footprint.x);
                var anchorY = random.Next(sectorStartY, sectorStartY + _gameplayConfig.NavigationSectorLengthCells - footprint.y + 1);
                var obstacle = new GeneratedGridObstacle(new Vector2Int(anchorX, anchorY), footprint, true, true);
                if (!CanPlace(obstacle, occupiedCells))
                {
                    continue;
                }

                PlaceObstacle(obstacle, occupiedCells, obstacles);
                pathObstacle = obstacle;
                return true;
            }

            pathObstacle = default;
            return false;
        }

        private void TryPlaceClusterObstacle(System.Random random, Vector2Int gridSize, int sectorStartY, bool[,] occupiedCells, List<GeneratedGridObstacle> obstacles, GeneratedGridObstacle pathObstacle)
        {
            var playerPathX = gridSize.x / 2;
            var sectorEndY = sectorStartY + _gameplayConfig.NavigationSectorLengthCells;
            for (var attempt = 0; attempt < _gameplayConfig.PlacementAttempts; attempt++)
            {
                var footprint = GetFootprint(random, false);
                var anchorX = Mathf.Clamp(
                    playerPathX + random.Next(-_gameplayConfig.NavigationPathClusterHalfWidthCells, _gameplayConfig.NavigationPathClusterHalfWidthCells + 1) - footprint.x / 2,
                    0,
                    gridSize.x - footprint.x);
                var anchorY = Mathf.Clamp(
                    pathObstacle.Anchor.y + random.Next(-_gameplayConfig.NavigationPathClusterHalfWidthCells, _gameplayConfig.NavigationPathClusterHalfWidthCells + 1),
                    sectorStartY,
                    sectorEndY - footprint.y);
                var obstacle = new GeneratedGridObstacle(new Vector2Int(anchorX, anchorY), footprint, true, false);
                if (OccupiesPathColumn(obstacle, playerPathX) || !CanPlace(obstacle, occupiedCells))
                {
                    continue;
                }

                PlaceObstacle(obstacle, occupiedCells, obstacles);
                return;
            }
        }

        private void PlaceRandomObstacles(System.Random random, Vector2Int gridSize, bool[,] occupiedCells, IReadOnlyList<GeneratedGridSector> sectors)
        {
            var playerPathX = gridSize.x / 2;
            for (var obstacleIndex = 0; obstacleIndex < _gameplayConfig.NavigationRandomObstacleCount; obstacleIndex++)
            {
                var placed = false;
                for (var attempt = 0; attempt < _gameplayConfig.PlacementAttempts; attempt++)
                {
                    var footprint = GetFootprint(random, false);
                    var anchorX = random.Next(0, gridSize.x - footprint.x + 1);
                    var anchorY = random.Next(_gameplayConfig.NavigationStartClearCells, gridSize.y - _gameplayConfig.NavigationDoorClearCells - footprint.y + 1);
                    var obstacle = new GeneratedGridObstacle(new Vector2Int(anchorX, anchorY), footprint, true, false);
                    if (OccupiesPlayerPath(obstacle, playerPathX) || !CanPlace(obstacle, occupiedCells))
                    {
                        continue;
                    }

                    MarkObstacleCells(obstacle, occupiedCells);
                    sectors[(anchorY - _gameplayConfig.NavigationStartClearCells) / _gameplayConfig.NavigationSectorLengthCells].AddObstacle(obstacle);
                    placed = true;
                    break;
                }

                if (!placed)
                {
                    throw new InvalidOperationException("Unable to place the configured number of random navigation obstacles outside the player path.");
                }
            }
        }

        private static bool OccupiesPathColumn(GeneratedGridObstacle obstacle, int playerPathX)
        {
            return obstacle.Anchor.x <= playerPathX && playerPathX < obstacle.Anchor.x + obstacle.Footprint.x;
        }

        private bool OccupiesPlayerPath(GeneratedGridObstacle obstacle, int playerPathX)
        {
            var cellWidth = _gameplayConfig.NavigationFieldSize.x / _gameplayConfig.NavigationGridSize.x;
            var playerRadiusInCells = Mathf.CeilToInt(_gameplayConfig.GetPlayerScale(_gameplayConfig.MaxEnergy) * 0.5f / cellWidth);
            var pathMinX = playerPathX - playerRadiusInCells;
            var pathMaxX = playerPathX + playerRadiusInCells;
            return obstacle.Anchor.x <= pathMaxX && obstacle.Anchor.x + obstacle.Footprint.x - 1 >= pathMinX;
        }

        private Vector2Int GetFootprint(System.Random random, bool mustBlockPlayerLine)
        {
            var areaIndex = mustBlockPlayerLine && _gameplayConfig.NavigationObstacleAreas.Count > 1
                ? random.Next(1, _gameplayConfig.NavigationObstacleAreas.Count)
                : random.Next(_gameplayConfig.NavigationObstacleAreas.Count);
            var area = _gameplayConfig.NavigationObstacleAreas[areaIndex];
            var side = Mathf.RoundToInt(Mathf.Sqrt(area));
            if (side * side != area)
            {
                throw new InvalidOperationException("Navigation obstacle areas must be square numbers, such as 1, 4, or 9.");
            }

            return new Vector2Int(side, side);
        }

        private static bool CanPlace(GeneratedGridObstacle obstacle, bool[,] occupiedCells)
        {
            for (var x = obstacle.Anchor.x; x < obstacle.Anchor.x + obstacle.Footprint.x; x++)
            {
                for (var y = obstacle.Anchor.y; y < obstacle.Anchor.y + obstacle.Footprint.y; y++)
                {
                    if (x < 0 || x >= occupiedCells.GetLength(0) || y < 0 || y >= occupiedCells.GetLength(1) || occupiedCells[x, y])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void PlaceObstacle(GeneratedGridObstacle obstacle, bool[,] occupiedCells, List<GeneratedGridObstacle> obstacles)
        {
            MarkObstacleCells(obstacle, occupiedCells);
            obstacles.Add(obstacle);
        }

        private static void MarkObstacleCells(GeneratedGridObstacle obstacle, bool[,] occupiedCells)
        {
            for (var x = obstacle.Anchor.x; x < obstacle.Anchor.x + obstacle.Footprint.x; x++)
            {
                for (var y = obstacle.Anchor.y; y < obstacle.Anchor.y + obstacle.Footprint.y; y++)
                {
                    occupiedCells[x, y] = true;
                }
            }
        }
    }

    public sealed class GeneratedGridLevelLayout
    {
        public GeneratedGridLevelLayout(IReadOnlyList<GeneratedGridSector> sectors)
        {
            Sectors = sectors;
        }

        public IReadOnlyList<GeneratedGridSector> Sectors { get; }
    }

    public sealed class GeneratedGridSector
    {
        private readonly List<GeneratedGridObstacle> _obstacles;

        public GeneratedGridSector(int index, int startY, int length, List<GeneratedGridObstacle> obstacles)
        {
            Index = index;
            StartY = startY;
            Length = length;
            _obstacles = obstacles;
        }

        public int Index { get; }
        public int StartY { get; }
        public int Length { get; }
        public IReadOnlyList<GeneratedGridObstacle> Obstacles => _obstacles;

        public void AddObstacle(GeneratedGridObstacle obstacle)
        {
            _obstacles.Add(obstacle);
        }
    }

    public readonly struct GeneratedGridObstacle
    {
        public GeneratedGridObstacle(Vector2Int anchor, Vector2Int footprint, bool blocksPlayer, bool isPathTarget)
        {
            Anchor = anchor;
            Footprint = footprint;
            BlocksPlayer = blocksPlayer;
            IsPathTarget = isPathTarget;
        }

        public Vector2Int Anchor { get; }
        public Vector2Int Footprint { get; }
        public bool BlocksPlayer { get; }
        public bool IsPathTarget { get; }
    }
}
