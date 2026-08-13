using Cysharp.Threading.Tasks;

namespace UtinComputerTest.Infrastructure.Progress.SaveLoad.Modules.Serializer {
    public interface ISerializer {
        UniTask<string> SerializeAsync<TData>(TData data);
        UniTask<TData> DeserializeAsync<TData>(string serializedData);
    }
}