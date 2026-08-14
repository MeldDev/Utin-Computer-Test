using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UtinComputerTest.Gameplay.Runtime;
using UtinComputerTest.Infrastructure._Services.SceneLoader;
using Zenject;

namespace UtinComputerTest.UI.Windows
{
    public sealed class WinWindowPresenter : IInitializable, IDisposable
    {
        private readonly IWindowService _windowService;
        private readonly IGameplayFlowService _gameplayFlowService;
        private readonly ISceneLoader _sceneLoader;
        private readonly CompositeDisposable _disposables = new();
        private bool _isViewBound;

        public WinWindowPresenter(
            IWindowService windowService,
            IGameplayFlowService gameplayFlowService,
            ISceneLoader sceneLoader)
        {
            _windowService = windowService;
            _gameplayFlowService = gameplayFlowService;
            _sceneLoader = sceneLoader;
        }

        public void Initialize()
        {
            _gameplayFlowService.LevelWon
                .Subscribe(_ => OpenWindow())
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OpenWindow()
        {
            var view = _windowService.OpenWindow<WinWindow>();
            if (_isViewBound)
            {
                return;
            }

            _isViewBound = true;
            view.NextLevelClicked
                .Subscribe(_ =>
                {
                    view.Close();
                    _gameplayFlowService.RequestNextLevel();
                })
                .AddTo(_disposables);

            view.MainMenuClicked
                .Subscribe(_ => _sceneLoader.LoadAsync(SceneID.MainMenu).Forget())
                .AddTo(_disposables);
        }
    }
}
