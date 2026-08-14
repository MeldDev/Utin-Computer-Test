using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UtinComputerTest.ScriptableObjects
{
    [CreateAssetMenu(menuName = "StaticData/Addresses/PrefabAddresses", fileName = "PrefabAddresses")]
    public sealed class PrefabAddresses : ScriptableObject
    {
        [field: Header("Map Windows")]
        [field: SerializeField] public AssetReferenceGameObject WinWindow { get; private set; }
        [field: SerializeField] public AssetReferenceGameObject LoseWindow { get; private set; }
    }
}
