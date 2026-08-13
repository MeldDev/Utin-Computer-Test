using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UtinComputerTest.UI.MainMenu
{
    public sealed class MenuView : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _exitButton;

        public IObservable<Unit> StartButtonClicked()
        {
            return _startButton.OnClickAsObservable();
        }

        public IObservable<Unit> ExitButtonClicked()
        {
            return _exitButton.OnClickAsObservable();
        }
    }
}
