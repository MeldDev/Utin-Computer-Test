using Cysharp.Threading.Tasks;

namespace UtinComputerTest.Infrastructure.Services.SceneLoading
{
    public interface ISceneContentReadiness
    {
        void BeginLoading(SceneID sceneId);
        void MarkReady(SceneID sceneId);
        UniTask WaitUntilReadyAsync(SceneID sceneId);
    }
}
