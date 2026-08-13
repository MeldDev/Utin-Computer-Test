using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace UtinComputerTest.Infrastructure.Progress.SaveLoad.Modules.Serializer {
    public class JsonSerializer : ISerializer {
        public UniTask<string> SerializeAsync<TData>(TData data) {
            string json = JsonConvert.SerializeObject(data);
            return UniTask.FromResult(json);
        }

        public UniTask<TData> DeserializeAsync<TData>(string json) {
            var data = JsonConvert.DeserializeObject<TData>(json);
            return UniTask.FromResult(data);
        }
    }
}
