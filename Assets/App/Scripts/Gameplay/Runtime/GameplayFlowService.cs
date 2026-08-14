using System;
using UniRx;

namespace UtinComputerTest.Gameplay.Runtime
{
    public sealed class GameplayFlowService : IGameplayFlowService, IDisposable
    {
        private readonly Subject<Unit> _levelWon = new();
        private readonly Subject<Unit> _levelLost = new();
        private readonly Subject<Unit> _nextLevelRequested = new();
        private readonly Subject<Unit> _levelRestartRequested = new();

        public IObservable<Unit> LevelWon => _levelWon;
        public IObservable<Unit> LevelLost => _levelLost;
        public IObservable<Unit> NextLevelRequested => _nextLevelRequested;
        public IObservable<Unit> LevelRestartRequested => _levelRestartRequested;

        public void ReportLevelWon()
        {
            _levelWon.OnNext(Unit.Default);
        }

        public void ReportLevelLost()
        {
            _levelLost.OnNext(Unit.Default);
        }

        public void RequestNextLevel()
        {
            _nextLevelRequested.OnNext(Unit.Default);
        }

        public void RequestLevelRestart()
        {
            _levelRestartRequested.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _levelWon.Dispose();
            _levelLost.Dispose();
            _nextLevelRequested.Dispose();
            _levelRestartRequested.Dispose();
        }
    }
}
