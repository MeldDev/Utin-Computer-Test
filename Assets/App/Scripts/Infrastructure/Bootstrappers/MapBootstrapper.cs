using UtinComputerTest.Gameplay.Configs;
using UtinComputerTest.Gameplay.Runtime;
using UnityEngine;
using Zenject;

namespace UtinComputerTest.Infrastructure.Bootstrappers
{
    public sealed class MapBootstrapper : IInitializable
    {
        private readonly GameplayConfig _gameplayConfig;
        private readonly LevelSequence _levelSequence;

        public MapBootstrapper(GameplayConfig gameplayConfig, LevelSequence levelSequence)
        {
            _gameplayConfig = gameplayConfig;
            _levelSequence = levelSequence;
        }

        public void Initialize()
        {
            var gameplayRoot = new GameObject("Gameplay Prototype");
            gameplayRoot.AddComponent<GameplayPrototypeController>().Initialize(_gameplayConfig, _levelSequence);
        }
    }
}
