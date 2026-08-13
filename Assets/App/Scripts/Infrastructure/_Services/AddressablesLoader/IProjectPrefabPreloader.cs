using Cysharp.Threading.Tasks;

namespace UtinComputerTest.Infrastructure._Services.AddressablesLoader
{
    public interface IProjectPrefabPreloader
    {
        UniTask PreloadAsync();
    }
}
