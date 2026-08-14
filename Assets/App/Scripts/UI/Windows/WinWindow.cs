using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UtinComputerTest.UI.Windows
{
    public sealed class WinWindow : AnimatedWindow
    {
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _mainMenuButton;

        public IObservable<Unit> NextLevelClicked => _nextLevelButton.OnClickAsObservable();
        public IObservable<Unit> MainMenuClicked => _mainMenuButton.OnClickAsObservable();
    }
}
