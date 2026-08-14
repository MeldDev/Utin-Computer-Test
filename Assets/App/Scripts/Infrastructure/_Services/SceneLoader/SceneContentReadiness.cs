using Cysharp.Threading.Tasks;

namespace UtinComputerTest.Infrastructure._Services.SceneLoader
{
    public sealed class SceneContentReadiness : ISceneContentReadiness
    {
        private UniTaskCompletionSource _mapReadySource = new();

        public void BeginLoading(SceneID sceneId)
        {
            if (sceneId == SceneID.Map)
            {
                _mapReadySource = new UniTaskCompletionSource();
            }
        }

        public void MarkReady(SceneID sceneId)
        {
            if (sceneId == SceneID.Map)
            {
                _mapReadySource.TrySetResult();
            }
        }

        public async UniTask WaitUntilReadyAsync(SceneID sceneId)
        {
            if (sceneId == SceneID.Map)
            {
                await _mapReadySource.Task;
            }
        }
    }
}
