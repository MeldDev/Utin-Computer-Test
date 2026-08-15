using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UtinComputerTest.Gameplay.Configs;
using UtinComputerTest.Gameplay.Runtime;
using UtinComputerTest.Gameplay.Views;
using UtinComputerTest.Infrastructure.Services.AddressableLoading;
using UtinComputerTest.Infrastructure.Services.SceneLoading;
using UtinComputerTest.ScriptableObjects;
using UtinComputerTest.UI.Windows;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace UtinComputerTest.Infrastructure.Bootstrappers
{
    public sealed class MapBootstrapper : IInitializable, IDisposable
    {
        private readonly GameplayAddressables _gameplayAddressables;
        private readonly IAddressableAssetProvider _assetProvider;
        private readonly GameplayDebugView _gameplayDebugView;
        private readonly GameplayCameraView _gameplayCameraView;
        private readonly PlayerPathService _playerPathService;
        private readonly IWindowFactory _windowFactory;
        private readonly IGameplayFlowService _gameplayFlowService;
        private readonly ISceneContentReadiness _sceneContentReadiness;
        private readonly List<AssetReference> _acquiredAssets = new();

        public MapBootstrapper(GameplayAddressables gameplayAddressables, IAddressableAssetProvider assetProvider, [InjectOptional] GameplayDebugView gameplayDebugView, GameplayCameraView gameplayCameraView, PlayerPathService playerPathService, IWindowFactory windowFactory, IGameplayFlowService gameplayFlowService, ISceneContentReadiness sceneContentReadiness)
        {
            _gameplayAddressables = gameplayAddressables;
            _assetProvider = assetProvider;
            _gameplayDebugView = gameplayDebugView;
            _gameplayCameraView = gameplayCameraView;
            _playerPathService = playerPathService;
            _windowFactory = windowFactory;
            _gameplayFlowService = gameplayFlowService;
            _sceneContentReadiness = sceneContentReadiness;
        }

        public void Initialize()
        {
            InitializeAsync().Forget();
        }

        public void Dispose()
        {
            foreach (var assetReference in _acquiredAssets)
            {
                _assetProvider.Release(assetReference);
            }

            _acquiredAssets.Clear();
        }

        private async UniTaskVoid InitializeAsync()
        {
            var configsCatalog = await AcquireAssetAsync<AddressableConfigsCatalog>(_gameplayAddressables.ConfigsCatalog);
            var gameplayConfig = await AcquireAssetAsync<GameplayConfig>(configsCatalog.GameplayConfig);
            var levelSequenceConfig = await AcquireAssetAsync<LevelSequence>(configsCatalog.LevelSequence);
            var prefabAddresses = await AcquireAssetAsync<PrefabAddresses>(configsCatalog.PrefabAddresses);
            var levels = await LoadLevelsAsync(levelSequenceConfig);
            var gameplayAssets = await LoadGameplayAssetsAsync(gameplayConfig);

            await _windowFactory.PreloadAsync(prefabAddresses);
            _playerPathService.Initialize(gameplayConfig);
            var gameplayRoot = new GameObject("Gameplay Prototype");
            gameplayRoot.AddComponent<GameplayPrototypeController>().Initialize(gameplayConfig, new GameplayLevelSequence(levels, levelSequenceConfig.Loop), gameplayAssets, _gameplayDebugView, _gameplayCameraView, _playerPathService, _gameplayFlowService);
            _sceneContentReadiness.MarkReady(SceneID.Map);
        }

        private async UniTask<List<LevelConfig>> LoadLevelsAsync(LevelSequence levelSequence)
        {
            var levels = new List<LevelConfig>(levelSequence.Levels.Count);
            foreach (var levelReference in levelSequence.Levels)
            {
                levels.Add(await AcquireAssetAsync<LevelConfig>(levelReference));
            }

            return levels;
        }

        private async UniTask<GameplayAssets> LoadGameplayAssetsAsync(GameplayConfig gameplayConfig)
        {
            var mapPrefab = await AcquireComponentAsync<MapView>(gameplayConfig.MapPrefab);
            var playerPrefab = await AcquireComponentAsync<PlayerBallView>(gameplayConfig.PlayerPrefab);
            var doorPrefab = await AcquireComponentAsync<DoorView>(gameplayConfig.DoorPrefab);
            var obstaclePrefab = await AcquireComponentAsync<ObstacleView>(gameplayConfig.ObstaclePrefab);
            var infectedObstacleMaterial = await AcquireAssetAsync<Material>(gameplayConfig.InfectedObstacleMaterial);
            var projectileMaterial = await AcquireAssetAsync<Material>(gameplayConfig.ProjectileMaterial);

            return new GameplayAssets(mapPrefab, playerPrefab, doorPrefab, obstaclePrefab, infectedObstacleMaterial, projectileMaterial);
        }

        private async UniTask<T> AcquireAssetAsync<T>(AssetReference assetReference) where T : UnityEngine.Object
        {
            var asset = await _assetProvider.AcquireAsync<T>(assetReference);
            _acquiredAssets.Add(assetReference);
            return asset;
        }

        private async UniTask<T> AcquireComponentAsync<T>(AssetReference assetReference) where T : Component
        {
            var component = await _assetProvider.AcquireComponentAsync<T>(assetReference);
            _acquiredAssets.Add(assetReference);
            return component;
        }
    }
}
