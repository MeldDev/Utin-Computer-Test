using UnityEngine;

namespace UtinComputerTest.UI.Providers
{
    public sealed class UIProvider : IUIProvider
    {
        public UIProvider(Canvas canvas, UILayers uiLayers)
        {
            Canvas = canvas;
            UILayers = uiLayers;
        }

        public Canvas Canvas { get; }
        public UILayers UILayers { get; }
    }
}
