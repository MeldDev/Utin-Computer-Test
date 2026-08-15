using Cysharp.Threading.Tasks;

namespace UtinComputerTest.Infrastructure.Services.AddressableLoading
{
    public interface IProjectPrefabPreloader
    {
        UniTask PreloadAsync();
    }
}
