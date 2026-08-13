using System;
using System.Collections.Generic;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Configs
{
    [CreateAssetMenu(menuName = "Utin Computer Test/Gameplay/Level Config", fileName = "LevelConfig")]
    public sealed class LevelConfig : ScriptableObject
    {
        [SerializeField] private int _levelId;
        [SerializeField] private int _seed;
        [SerializeField] private float _initialEnergy = 100f;
        [SerializeField] private float _expectedEnergyUsage = 80f;
        [SerializeField] private List<SectorConfig> _sectors = new();
        [SerializeField] private Vector2 _distanceBetweenSectors = new(6f, 12f);
        [SerializeField] private float _distanceFromLastSectorToDoor = 8f;
        [SerializeField] private float _roadWidth = 8f;

        public int LevelId => _levelId;
        public int Seed => _seed;
        public float InitialEnergy => _initialEnergy;
        public float ExpectedEnergyUsage => _expectedEnergyUsage;
        public IReadOnlyList<SectorConfig> Sectors => _sectors;
        public Vector2 DistanceBetweenSectors => _distanceBetweenSectors;
        public float DistanceFromLastSectorToDoor => _distanceFromLastSectorToDoor;
        public float RoadWidth => _roadWidth;
    }

    [Serializable]
    public sealed class SectorConfig
    {
        [Min(0f)] public float energyBudget = 20f;
        public Vector2Int expectedShotsRange = new(1, 2);
        public Vector2Int obstaclesPerCluster = new(3, 6);
        public Vector2 clusterSpacingRange = new(2.5f, 4f);
        public Vector2 sectorWidthRange = new(4f, 7f);
        public Vector2 distanceToNextSector = new(6f, 12f);
    }
}
