using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UtinComputerTest.Infrastructure.Services.AddressableLoading;
using UtinComputerTest.Infrastructure.StateMachine.States.Base;

namespace UtinComputerTest.Infrastructure.StateMachine.States.Project
{
    public sealed class ProjectPrefabsPreloadState : IState
    {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly List<IProjectPrefabPreloader> _prefabPreloaders;

        public ProjectPrefabsPreloadState(
            IGameStateMachine gameStateMachine,
            List<IProjectPrefabPreloader> prefabPreloaders)
        {
            _gameStateMachine = gameStateMachine;
            _prefabPreloaders = prefabPreloaders;
        }

        public void Enter()
        {
            PreloadAsync().Forget();
        }

        private async UniTaskVoid PreloadAsync()
        {
            await UniTask.WhenAll(_prefabPreloaders.Select(preloader => preloader.PreloadAsync()));
            _gameStateMachine.EnterState<ProjectInitializationState>();
        }
    }
}
