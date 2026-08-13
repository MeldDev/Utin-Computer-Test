using System;
using UniRx;
using UnityEngine;

namespace UtinComputerTest.Gameplay.Runtime
{
    public sealed class GameplayInputView : MonoBehaviour
    {
        private readonly Subject<Unit> _pressed = new();
        private readonly Subject<Unit> _released = new();
        private readonly Subject<Unit> _restartRequested = new();

        public IObservable<Unit> Pressed => _pressed;
        public IObservable<Unit> Released => _released;
        public IObservable<Unit> RestartRequested => _restartRequested;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _pressed.OnNext(Unit.Default);
            }

            if (Input.GetMouseButtonUp(0))
            {
                _released.OnNext(Unit.Default);
            }

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    _pressed.OnNext(Unit.Default);
                }

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    _released.OnNext(Unit.Default);
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                _restartRequested.OnNext(Unit.Default);
            }
        }

        private void OnDestroy()
        {
            _pressed.OnCompleted();
            _released.OnCompleted();
            _restartRequested.OnCompleted();
            _pressed.Dispose();
            _released.Dispose();
            _restartRequested.Dispose();
        }
    }
}
