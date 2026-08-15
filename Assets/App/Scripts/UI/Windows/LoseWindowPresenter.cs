using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UtinComputerTest.Gameplay.Runtime;
using UtinComputerTest.Infrastructure.Services.SceneLoading;
using Zenject;

namespace UtinComputerTest.UI.Windows
{
    public sealed class LoseWindowPresenter : IInitializable, IDisposable
    {
        private readonly IWindowService _windowService;
        private readonly IGameplayFlowService _gameplayFlowService;
        private readonly ISceneLoader _sceneLoader;
        private readonly CompositeDisposable _disposables = new();
        private bool _isViewBound;

        public LoseWindowPresenter(
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
            _gameplayFlowService.LevelLost
                .Subscribe(_ => OpenWindow())
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OpenWindow()
        {
            var view = _windowService.OpenWindow<LoseWindow>();
            if (_isViewBound)
            {
                return;
            }

            _isViewBound = true;
            view.RetryClicked
                .Subscribe(_ =>
                {
                    view.Close();
                    _gameplayFlowService.RequestLevelRestart();
                })
                .AddTo(_disposables);

            view.MainMenuClicked
                .Subscribe(_ => _sceneLoader.LoadAsync(SceneID.MainMenu).Forget())
                .AddTo(_disposables);
        }
    }
}
