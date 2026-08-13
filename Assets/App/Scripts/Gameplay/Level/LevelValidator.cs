using System.Collections.Generic;
using System.Linq;
using UtinComputerTest.Gameplay.Configs;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Level
{
    public sealed class LevelValidator
    {
        private readonly GameplayConfig _gameplayConfig;

        public LevelValidator(GameplayConfig gameplayConfig)
        {
            _gameplayConfig = gameplayConfig;
        }

        public ValidationResult Validate(LevelConfig levelConfig, GeneratedLevelLayout layout)
        {
            var warnings = new List<string>();
            var expectedUsage = levelConfig.Sectors.Sum(sector => sector.energyBudget);
            var reserve = levelConfig.InitialEnergy - expectedUsage;
            if (!Mathf.Approximately(expectedUsage, levelConfig.ExpectedEnergyUsage))
            {
                warnings.Add($"Expected usage is {expectedUsage:0.##}, but target is {levelConfig.ExpectedEnergyUsage:0.##}.");
            }

            if (reserve < levelConfig.InitialEnergy * 0.2f)
            {
                warnings.Add($"Expected reserve is {reserve:0.##}; it is below 20%.");
            }

            if (layout.DoorPosition.z <= 0f)
            {
                warnings.Add("Door position is invalid.");
            }

            foreach (var sector in layout.Sectors)
            {
                foreach (var cluster in sector.Clusters)
                {
                    if (!IsConnected(cluster))
                    {
                        warnings.Add($"Sector {sector.Index} contains a disconnected cluster.");
                    }

                    if (HasOverlaps(cluster))
                    {
                        warnings.Add($"Sector {sector.Index} contains overlapping obstacles.");
                    }
                }
            }

            return new ValidationResult(expectedUsage, reserve, warnings);
        }

        private static bool IsConnected(GeneratedCluster cluster)
        {
            if (cluster.ObstaclePositions.Count == 0)
            {
                return false;
            }

            var visited = new HashSet<int> { 0 };
            var pending = new Queue<int>();
            pending.Enqueue(0);
            while (pending.Count > 0)
            {
                var current = pending.Dequeue();
                for (var index = 0; index < cluster.ObstaclePositions.Count; index++)
                {
                    if (!visited.Contains(index) && Vector3.Distance(cluster.ObstaclePositions[current], cluster.ObstaclePositions[index]) <= cluster.RequiredInfectionRadius)
                    {
                        visited.Add(index);
                        pending.Enqueue(index);
                    }
                }
            }

            return visited.Count == cluster.ObstaclePositions.Count;
        }

        private bool HasOverlaps(GeneratedCluster cluster)
        {
            var minDistance = _gameplayConfig.ObstacleRadius * 2f;
            for (var first = 0; first < cluster.ObstaclePositions.Count; first++)
            {
                for (var second = first + 1; second < cluster.ObstaclePositions.Count; second++)
                {
                    if (Vector3.Distance(cluster.ObstaclePositions[first], cluster.ObstaclePositions[second]) < minDistance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public sealed class ValidationResult
    {
        public ValidationResult(float expectedUsage, float reserve, IReadOnlyList<string> warnings)
        {
            ExpectedUsage = expectedUsage;
            Reserve = reserve;
            Warnings = warnings;
        }

        public float ExpectedUsage { get; }
        public float Reserve { get; }
        public IReadOnlyList<string> Warnings { get; }
        public bool IsValid => Warnings.Count == 0;
    }
}
