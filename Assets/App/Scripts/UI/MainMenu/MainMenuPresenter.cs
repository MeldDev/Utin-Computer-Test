using System;
using Cysharp.Threading.Tasks;
using UtinComputerTest.Infrastructure._Services.SceneLoader;
using UniRx;
using UnityEngine;
using Zenject;

namespace UtinComputerTest.UI.MainMenu
{
    public sealed class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly MenuView _view;
        private readonly ISceneLoader _sceneLoader;
        private readonly CompositeDisposable _disposables = new();

        public MainMenuPresenter(MenuView view, ISceneLoader sceneLoader)
        {
            _view = view;
            _sceneLoader = sceneLoader;
        }

        public void Initialize()
        {
            _view.StartButtonClicked()
                .Subscribe(_ => OpenMap())
                .AddTo(_disposables);

            _view.ExitButtonClicked()
                .Subscribe(_ => Application.Quit())
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OpenMap()
        {
            _sceneLoader.LoadAsync(SceneID.Map).Forget();
        }
    }
}
