using Cysharp.Threading.Tasks;

namespace UtinComputerTest.Infrastructure._Services.SceneLoader
{
    public interface ISceneContentReadiness
    {
        void BeginLoading(SceneID sceneId);
        void MarkReady(SceneID sceneId);
        UniTask WaitUntilReadyAsync(SceneID sceneId);
    }
}
