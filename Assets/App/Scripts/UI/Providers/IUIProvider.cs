using UnityEngine;

namespace UtinComputerTest.UI.Providers
{
    public interface IUIProvider
    {
        Canvas Canvas { get; }
        UILayers UILayers { get; }
    }
}
