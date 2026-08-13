using UtinComputerTest.Infrastructure.Bootstrappers;
using UtinComputerTest.Gameplay.Configs;
using UnityEngine;
using Zenject;

namespace UtinComputerTest.Infrastructure.Installers.Scene
{
    public sealed class MapSceneInstaller : MonoInstaller
    {
        [SerializeField] private GameplayConfig _gameplayConfig;
        [SerializeField] private LevelSequence _levelSequence;

        public override void InstallBindings()
        {
            Container.BindInstance(_gameplayConfig).AsSingle();
            Container.BindInstance(_levelSequence).AsSingle();
            Container.BindInterfacesTo<MapBootstrapper>().AsSingle();
        }
    }
}
