using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UtinComputerTest.Infrastructure.Progress.SaveParts
{
    public class PlayerPrefsSaveBackend : ISaveBackend
    {
        public UniTask<bool> ExistsAsync(string key)
        {
            return UniTask.FromResult(PlayerPrefs.HasKey(key));
        }

        public UniTask<string> ReadAsync(string key)
        {
            return UniTask.FromResult(PlayerPrefs.GetString(key));
        }

        public UniTask WriteAsync(string key, string serializedState)
        {
            PlayerPrefs.SetString(key, serializedState);
            return UniTask.CompletedTask;
        }

        public UniTask DeleteAsync(string key)
        {
            PlayerPrefs.DeleteKey(key);
            return UniTask.CompletedTask;
        }

        public UniTask FlushAsync()
        {
            PlayerPrefs.Save();
            return UniTask.CompletedTask;
        }
    }
}
