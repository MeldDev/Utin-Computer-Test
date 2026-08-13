using Cysharp.Threading.Tasks;

namespace UtinComputerTest.Infrastructure.Progress.SaveParts
{
    public interface ISaveBackend
    {
        public UniTask<bool> ExistsAsync(string key);
        public UniTask<string> ReadAsync(string key);
        public UniTask WriteAsync(string key, string serializedState);
        public UniTask DeleteAsync(string key);
        public UniTask FlushAsync();
    }
}
