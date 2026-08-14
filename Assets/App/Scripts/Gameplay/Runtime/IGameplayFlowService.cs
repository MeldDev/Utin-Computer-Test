using System;
using UniRx;

namespace UtinComputerTest.Gameplay.Runtime
{
    public interface IGameplayFlowService
    {
        IObservable<Unit> LevelWon { get; }
        IObservable<Unit> LevelLost { get; }
        IObservable<Unit> NextLevelRequested { get; }
        IObservable<Unit> LevelRestartRequested { get; }
        void ReportLevelWon();
        void ReportLevelLost();
        void RequestNextLevel();
        void RequestLevelRestart();
    }
}
