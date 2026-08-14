using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UtinComputerTest.Infrastructure._Services.AddressablesLoader;
using UtinComputerTest.ScriptableObjects;
using UtinComputerTest.UI.Providers;
using UnityEngine;
using Zenject;

namespace UtinComputerTest.UI.Windows
{
    public sealed class MapWindowFactory : IWindowFactory
    {
        private readonly IAddressableAssetProvider _assetProvider;
        private readonly IUIProvider _uiProvider;
        private readonly DiContainer _container;
        private readonly Dictionary<Type, BaseWindow> _prefabs = new();
        private PrefabAddresses _prefabAddresses;

        public MapWindowFactory(
            IAddressableAssetProvider assetProvider,
            IUIProvider uiProvider,
            DiContainer container)
        {
            _assetProvider = assetProvider;
            _uiProvider = uiProvider;
            _container = container;
        }

        public async UniTask PreloadAsync(PrefabAddresses prefabAddresses)
        {
            _prefabAddresses = prefabAddresses;
            await _assetProvider.LoadComponentAsync<WinWindow>(_prefabAddresses.WinWindow);
            await _assetProvider.LoadComponentAsync<LoseWindow>(_prefabAddresses.LoseWindow);
        }

        public TWindow CreateWindow<TWindow>() where TWindow : BaseWindow
        {
            var windowType = typeof(TWindow);
            var prefab = GetPrefab<TWindow>(windowType);
            var instance = _container.InstantiatePrefabForComponent<TWindow>(prefab, GetLayerParent(prefab.Layer));
            instance.gameObject.SetActive(false);
            return instance;
        }

        private TWindow GetPrefab<TWindow>(Type windowType) where TWindow : BaseWindow
        {
            if (_prefabs.TryGetValue(windowType, out var cachedPrefab))
            {
                return (TWindow)cachedPrefab;
            }

            var prefabReference = GetPrefabReference(windowType);
            var prefab = _assetProvider.GetLoadedComponent<TWindow>(prefabReference);
            _prefabs.Add(windowType, prefab);
            return prefab;
        }

        private Transform GetLayerParent(WindowLayer layer)
        {
            return layer switch
            {
                0 => _uiProvider.UILayers.Normal,
                WindowLayer.FullscreenFade => _uiProvider.UILayers.FullscreenFade,
                WindowLayer.Normal => _uiProvider.UILayers.Normal,
                WindowLayer.Navbar => _uiProvider.UILayers.Navbar,
                WindowLayer.TopPanel => _uiProvider.UILayers.TopPanel,
                WindowLayer.TopAndNavbar => _uiProvider.UILayers.TopAndNavbar,
                WindowLayer.Tutorial => _uiProvider.UILayers.Tutorial,
                _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null)
            };
        }

        private UnityEngine.AddressableAssets.AssetReferenceGameObject GetPrefabReference(Type windowType)
        {
            if (windowType == typeof(WinWindow))
            {
                return _prefabAddresses.WinWindow;
            }

            if (windowType == typeof(LoseWindow))
            {
                return _prefabAddresses.LoseWindow;
            }

            throw new ArgumentException($"Window prefab for '{windowType.FullName}' is not configured.", nameof(windowType));
        }
    }
}
