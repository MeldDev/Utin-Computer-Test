using System.Collections.Generic;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Configs
{
    [CreateAssetMenu(menuName = "Utin Computer Test/Gameplay/Level Sequence", fileName = "LevelSequence")]
    public sealed class LevelSequence : ScriptableObject
    {
        [SerializeField] private List<LevelConfig> _levels = new();
        [SerializeField] private bool _loop = true;

        public int Count => _levels.Count;

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
