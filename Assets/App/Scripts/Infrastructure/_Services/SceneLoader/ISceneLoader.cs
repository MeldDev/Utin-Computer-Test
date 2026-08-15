using Cysharp.Threading.Tasks;

namespace UtinComputerTest.Infrastructure.Services.SceneLoading
{
    public interface ISceneLoader
    {
        UniTask LoadAsync(SceneID sceneId);
    }
}
