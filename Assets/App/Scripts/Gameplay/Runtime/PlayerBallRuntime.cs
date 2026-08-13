using UtinComputerTest.Gameplay.Configs;
using UtinComputerTest.Gameplay.Views;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Runtime
{
    public sealed class PlayerBallRuntime
    {
        private readonly PlayerBallView _view;
        private readonly GameplayConfig _config;
        private float _energy;

        public PlayerBallRuntime(PlayerBallView view, GameplayConfig config)
        {
            _view = view;
            _config = config;
        }

        public float Energy => _energy;
        public Vector3 Position => _view.Position;

        public void Reset(float energy)
        {
            _energy = Mathf.Clamp(energy, 0f, _config.MaxEnergy);
            UpdateVisual();
        }

        public float TransferEnergy(float requestedEnergy)
        {
            var transferredEnergy = Mathf.Min(requestedEnergy, _energy);
            _energy -= transferredEnergy;
            UpdateVisual();
            return transferredEnergy;
        }

        public void SetPosition(Vector3 position)
        {
            _view.SetPosition(position);
        }

        private void UpdateVisual()
        {
            _view.SetScale(_config.GetPlayerScale(_energy));
        }
    }
}
