using UnityEngine;
using UtinComputerTest.Gameplay.Views;

namespace UtinComputerTest.Gameplay.Configs
{
    public sealed class GameplayAssets
    {
        public GameplayAssets(MapView mapPrefab, PlayerBallView playerPrefab, DoorView doorPrefab, ObstacleView obstaclePrefab, Material infectedObstacleMaterial, Material projectileMaterial)
        {
            MapPrefab = mapPrefab;
            PlayerPrefab = playerPrefab;
            DoorPrefab = doorPrefab;
            ObstaclePrefab = obstaclePrefab;
            InfectedObstacleMaterial = infectedObstacleMaterial;
            ProjectileMaterial = projectileMaterial;
        }

        public MapView MapPrefab { get; }
        public PlayerBallView PlayerPrefab { get; }
        public DoorView DoorPrefab { get; }
        public ObstacleView ObstaclePrefab { get; }
        public Material InfectedObstacleMaterial { get; }
        public Material ProjectileMaterial { get; }
    }
}
