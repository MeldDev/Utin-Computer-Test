using Cysharp.Threading.Tasks;

namespace UtinComputerTest.Infrastructure._Services.SceneLoader
{
    public interface ISceneLoader
    {
        UniTask LoadAsync(SceneID sceneId);
    }
}
