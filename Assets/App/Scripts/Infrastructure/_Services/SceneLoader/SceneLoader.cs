using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UtinComputerTest.Infrastructure._Services.SceneLoader
{
    public sealed class SceneLoader : ISceneLoader
    {
        private const string LoadingSceneName = "Loading";

        public async UniTask LoadAsync(SceneID sceneId)
        {
            await SceneManager.LoadSceneAsync(LoadingSceneName, LoadSceneMode.Single);
            await UniTask.NextFrame();
            await SceneManager.LoadSceneAsync(sceneId.ToString(), LoadSceneMode.Single);
        }
    }
}
