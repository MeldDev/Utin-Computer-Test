using UnityEngine;
using UnityEngine.AddressableAssets;
using UtinComputerTest.ScriptableObjects;

namespace UtinComputerTest.Gameplay.Configs
{
    [CreateAssetMenu(menuName = "Utin Computer Test/Configs/Addressable Configs Catalog", fileName = "AddressableConfigsCatalog")]
    public sealed class AddressableConfigsCatalog : ScriptableObject
    {
        [SerializeField] private AssetReferenceT<GameplayConfig> _gameplayConfig;
        [SerializeField] private AssetReferenceT<LevelSequence> _levelSequence;
        [SerializeField] private AssetReferenceT<PrefabAddresses> _prefabAddresses;

        public AssetReferenceT<GameplayConfig> GameplayConfig => _gameplayConfig;
        public AssetReferenceT<LevelSequence> LevelSequence => _levelSequence;
        public AssetReferenceT<PrefabAddresses> PrefabAddresses => _prefabAddresses;
    }
}
