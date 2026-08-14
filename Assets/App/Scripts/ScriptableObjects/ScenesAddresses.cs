using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UtinComputerTest.ScriptableObjects
{
    [CreateAssetMenu(menuName = "StaticData/Addresses/ScenesAddresses", fileName = "ScenesAddresses")]
    public sealed class ScenesAddresses : ScriptableObject
    {
        [field: SerializeField] public AssetReference MainMenu { get; private set; }
        [field: SerializeField] public AssetReference Map { get; private set; }
        [field: SerializeField] public AssetReference Loading { get; private set; }
    }
}
