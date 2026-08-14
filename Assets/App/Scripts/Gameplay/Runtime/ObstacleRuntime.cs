using System;
using UniRx;
using UtinComputerTest.Gameplay.Views;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Runtime
{
    public enum ObstacleState
    {
        Normal,
        Infected,
        Destroyed
    }

    public sealed class ObstacleRuntime
    {
        private readonly ObstacleView _view;
        private readonly float _radius;
        private readonly Subject<ObstacleRuntime> _destroyed = new();

        public ObstacleRuntime(ObstacleView view, float radius, bool blocksPlayer, bool isPathTarget, Vector2Int gridAnchor, Vector2Int gridFootprint)
        {
            _view = view;
            _radius = radius;
            BlocksPlayer = blocksPlayer;
            IsPathTarget = isPathTarget;
            GridAnchor = gridAnchor;
            GridFootprint = gridFootprint;
        }

        public ObstacleState State { get; private set; }
        public Vector3 Position => _view.Position;
        public float Radius => _radius;
        public bool BlocksPlayer { get; }
        public bool IsPathTarget { get; }
        public Vector2Int GridAnchor { get; }
        public Vector2Int GridFootprint { get; }
        public ObstacleView View => _view;
        public IObservable<ObstacleRuntime> Destroyed => _destroyed;

        public bool Infect()
        {
            if (State != ObstacleState.Normal)
            {
                return false;
            }

            State = ObstacleState.Infected;
            _view.PlayInfection();
            return true;
        }

        public void Destroy()
        {
            if (State == ObstacleState.Destroyed)
            {
                return;
            }

            State = ObstacleState.Destroyed;
            _destroyed.OnNext(this);
            _destroyed.OnCompleted();
            _view.gameObject.SetActive(false);
        }
    }
}
