using UnityEngine.AddressableAssets;

namespace UtinComputerTest.Gameplay.Configs
{
    public sealed class GameplayAddressables
    {
        public GameplayAddressables(AssetReferenceT<AddressableConfigsCatalog> configsCatalog)
        {
            ConfigsCatalog = configsCatalog;
        }

        public AssetReferenceT<AddressableConfigsCatalog> ConfigsCatalog { get; }
    }
}
