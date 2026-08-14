using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UtinComputerTest.UI.Windows
{
    public sealed class LoseWindow : AnimatedWindow
    {
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;

        public IObservable<Unit> RetryClicked => _retryButton.OnClickAsObservable();
        public IObservable<Unit> MainMenuClicked => _mainMenuButton.OnClickAsObservable();
    }
}
