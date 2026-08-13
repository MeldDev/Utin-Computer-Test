using System;
using System.Collections.Generic;
using UtinComputerTest.Gameplay.Configs;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Level
{
    public sealed class LevelLayoutGenerator
    {
        private readonly GameplayConfig _gameplayConfig;

        public LevelLayoutGenerator(GameplayConfig gameplayConfig)
        {
            _gameplayConfig = gameplayConfig;
        }

        public GeneratedLevelLayout Generate(LevelConfig levelConfig)
        {
            var random = new System.Random(levelConfig.Seed);
            var sectors = new List<GeneratedSector>();
            var cursor = 5f;

            for (var sectorIndex = 0; sectorIndex < levelConfig.Sectors.Count; sectorIndex++)
            {
                var sectorConfig = levelConfig.Sectors[sectorIndex];
                var shots = NextInclusive(random, sectorConfig.expectedShotsRange);
                var budgets = SplitBudget(random, sectorConfig.energyBudget, shots);
                var clusters = new List<GeneratedCluster>();
                var sectorStart = cursor;

                for (var clusterIndex = 0; clusterIndex < budgets.Count; clusterIndex++)
                {
                    var cluster = GenerateCluster(random, levelConfig, sectorConfig, budgets[clusterIndex], cursor);
                    clusters.Add(cluster);
                    cursor += Mathf.Max(Next(random, sectorConfig.clusterSpacingRange), cluster.RequiredInfectionRadius * _gameplayConfig.ClusterSeparationMultiplier);
                }

                var distance = Next(random, sectorConfig.distanceToNextSector);
                cursor += distance;
                sectors.Add(new GeneratedSector(sectorIndex, sectorStart - _gameplayConfig.PlayerStopDistanceBeforeSector, clusters));
            }

            return new GeneratedLevelLayout(sectors, new Vector3(0f, 0.75f, cursor + levelConfig.DistanceFromLastSectorToDoor), cursor + levelConfig.DistanceFromLastSectorToDoor + 4f);
        }

        private GeneratedCluster GenerateCluster(System.Random random, LevelConfig levelConfig, SectorConfig sectorConfig, float energy, float zPosition)
        {
            var radius = _gameplayConfig.GetInfectionRadius(energy);
            var positions = new List<Vector3>();
            var obstacleCount = NextInclusive(random, sectorConfig.obstaclesPerCluster);
            var halfRoad = levelConfig.RoadWidth * 0.5f - _gameplayConfig.ObstacleRadius;
            var anchor = new Vector3(Next(random, new Vector2(-halfRoad * 0.35f, halfRoad * 0.35f)), _gameplayConfig.ObstacleRadius, zPosition);
            positions.Add(anchor);

            for (var obstacleIndex = 1; obstacleIndex < obstacleCount; obstacleIndex++)
            {
                var placed = false;
                for (var attempt = 0; attempt < _gameplayConfig.PlacementAttempts; attempt++)
                {
                    var parent = positions[random.Next(positions.Count)];
                    var angle = (float)random.NextDouble() * Mathf.PI * 2f;
                    var distance = radius * Next(random, new Vector2(_gameplayConfig.ClusterMinDistanceFactor, _gameplayConfig.ClusterMaxDistanceFactor));
                    var candidate = parent + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
                    candidate.x = Mathf.Clamp(candidate.x, -halfRoad, halfRoad);

                    if (IsFree(candidate, positions))
                    {
                        positions.Add(candidate);
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                {
                    positions.Add(positions[positions.Count - 1] + Vector3.right * _gameplayConfig.ObstacleRadius * 2.1f);
                }
            }

            return new GeneratedCluster(energy, radius, positions);
        }

        private bool IsFree(Vector3 candidate, List<Vector3> positions)
        {
            var minDistance = _gameplayConfig.ObstacleRadius * 2.05f;
            foreach (var position in positions)
            {
                if (Vector3.Distance(candidate, position) < minDistance)
                {
                    return false;
                }
            }

            return true;
        }

        private static List<float> SplitBudget(System.Random random, float budget, int count)
        {
            var weights = new float[count];
            var totalWeight = 0f;
            for (var index = 0; index < count; index++)
            {
                weights[index] = 0.75f + (float)random.NextDouble() * 0.5f;
                totalWeight += weights[index];
            }

            var result = new List<float>(count);
            for (var index = 0; index < count; index++)
            {
                result.Add(budget * weights[index] / totalWeight);
            }

            return result;
        }

        private static int NextInclusive(System.Random random, Vector2Int range)
        {
            return random.Next(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y) + 1);
        }

        private static float Next(System.Random random, Vector2 range)
        {
            return Mathf.Lerp(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y), (float)random.NextDouble());
        }
    }

    public sealed class GeneratedLevelLayout
    {
        public GeneratedLevelLayout(IReadOnlyList<GeneratedSector> sectors, Vector3 doorPosition, float roadLength)
        {
            Sectors = sectors;
            DoorPosition = doorPosition;
            RoadLength = roadLength;
        }

        public IReadOnlyList<GeneratedSector> Sectors { get; }
        public Vector3 DoorPosition { get; }
        public float RoadLength { get; }
    }

    public sealed class GeneratedSector
    {
        public GeneratedSector(int index, float stopPositionZ, IReadOnlyList<GeneratedCluster> clusters)
        {
            Index = index;
            StopPositionZ = stopPositionZ;
            Clusters = clusters;
        }

        public int Index { get; }
        public float StopPositionZ { get; }
        public IReadOnlyList<GeneratedCluster> Clusters { get; }
    }

    public sealed class GeneratedCluster
    {
        public GeneratedCluster(float expectedEnergy, float requiredInfectionRadius, IReadOnlyList<Vector3> obstaclePositions)
        {
            ExpectedEnergy = expectedEnergy;
            RequiredInfectionRadius = requiredInfectionRadius;
            ObstaclePositions = obstaclePositions;
        }

        public float ExpectedEnergy { get; }
        public float RequiredInfectionRadius { get; }
        public IReadOnlyList<Vector3> ObstaclePositions { get; }
    }
}
