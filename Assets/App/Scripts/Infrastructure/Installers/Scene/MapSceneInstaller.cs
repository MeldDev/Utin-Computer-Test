using UtinComputerTest.Infrastructure.Bootstrappers;
using UtinComputerTest.Gameplay.Configs;
using UtinComputerTest.Gameplay.Runtime;
using UtinComputerTest.Gameplay.Views;
using UnityEngine;
using Zenject;

namespace UtinComputerTest.Infrastructure.Installers.Scene
{
    public sealed class MapSceneInstaller : MonoInstaller
    {
        [SerializeField] private GameplayConfig _gameplayConfig;
        [SerializeField] private LevelSequence _levelSequence;
        [SerializeField] private GameplayDebugView _gameplayDebugView;
        [SerializeField] private GameplayCameraView _gameplayCameraView;

        public override void InstallBindings()
        {
            Container.BindInstance(_gameplayConfig).AsSingle();
            Container.BindInstance(_levelSequence).AsSingle();
            Container.Bind<PlayerPathService>().AsSingle();
            if (_gameplayDebugView != null)
            {
                Container.BindInstance(_gameplayDebugView).AsSingle();
            }
            Container.BindInstance(_gameplayCameraView).AsSingle();
            Container.BindInterfacesTo<MapBootstrapper>().AsSingle();
        }
    }
}
