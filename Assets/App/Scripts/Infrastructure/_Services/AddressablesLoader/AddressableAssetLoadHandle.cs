using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UtinComputerTest.Infrastructure._Services.AddressablesLoader
{
    public readonly struct AddressableAssetLoadHandle<T> where T : Object
    {
        public AddressableAssetLoadHandle(T asset, AsyncOperationHandle operationHandle)
        {
            Asset = asset;
            OperationHandle = operationHandle;
        }

        public T Asset { get; }
        public AsyncOperationHandle OperationHandle { get; }
    }
}
