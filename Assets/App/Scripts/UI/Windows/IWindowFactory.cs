using Cysharp.Threading.Tasks;
using UtinComputerTest.ScriptableObjects;

namespace UtinComputerTest.UI.Windows
{
    public interface IWindowFactory
    {
        UniTask PreloadAsync(PrefabAddresses prefabAddresses);
        TWindow CreateWindow<TWindow>() where TWindow : BaseWindow;
    }
}
