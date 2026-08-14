using UtinComputerTest.Gameplay.Configs;
using UtinComputerTest.Gameplay.Runtime;
using UtinComputerTest.Gameplay.Views;
using UnityEngine;
using Zenject;

namespace UtinComputerTest.Infrastructure.Bootstrappers
{
    public sealed class MapBootstrapper : IInitializable
    {
        private readonly GameplayConfig _gameplayConfig;
        private readonly LevelSequence _levelSequence;
        private readonly GameplayDebugView _gameplayDebugView;
        private readonly GameplayCameraView _gameplayCameraView;
        private readonly PlayerPathService _playerPathService;

        public MapBootstrapper(GameplayConfig gameplayConfig, LevelSequence levelSequence, [InjectOptional] GameplayDebugView gameplayDebugView, GameplayCameraView gameplayCameraView, PlayerPathService playerPathService)
        {
            _gameplayConfig = gameplayConfig;
            _levelSequence = levelSequence;
            _gameplayDebugView = gameplayDebugView;
            _gameplayCameraView = gameplayCameraView;
            _playerPathService = playerPathService;
        }

        public void Initialize()
        {
            var gameplayRoot = new GameObject("Gameplay Prototype");
            gameplayRoot.AddComponent<GameplayPrototypeController>().Initialize(_gameplayConfig, _levelSequence, _gameplayDebugView, _gameplayCameraView, _playerPathService);
        }
    }
}
