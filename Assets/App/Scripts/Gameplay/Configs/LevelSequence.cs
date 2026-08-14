using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UtinComputerTest.Gameplay.Configs
{
    [CreateAssetMenu(menuName = "Utin Computer Test/Gameplay/Level Sequence", fileName = "LevelSequence")]
    public sealed class LevelSequence : ScriptableObject
    {
        [SerializeField] private List<AssetReferenceT<LevelConfig>> _levels = new();
        [SerializeField] private bool _loop = true;

        public IReadOnlyList<AssetReferenceT<LevelConfig>> Levels => _levels;
        public bool Loop => _loop;
    }
}
