using System.Collections.Generic;

namespace UtinComputerTest.Gameplay.Configs
{
    public sealed class GameplayLevelSequence
    {
        private readonly IReadOnlyList<LevelConfig> _levels;
        private readonly bool _loop;

        public GameplayLevelSequence(IReadOnlyList<LevelConfig> levels, bool loop)
        {
            _levels = levels;
            _loop = loop;
        }

        public LevelConfig GetLevel(int index)
        {
            if (_levels.Count == 0)
            {
                return null;
            }

            if (_loop)
            {
                return _levels[index % _levels.Count];
            }

            return index < _levels.Count ? _levels[index] : null;
        }
    }
}
