using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UtinComputerTest.Infrastructure.Services.AddressableLoading
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
