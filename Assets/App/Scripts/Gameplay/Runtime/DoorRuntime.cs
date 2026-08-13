using UnityEngine;
using UtinComputerTest.Gameplay.Views;

namespace UtinComputerTest.Gameplay.Runtime
{
    public sealed class DoorRuntime
    {
        private readonly DoorView _view;

        public DoorRuntime(DoorView view)
        {
            _view = view;
        }

        public bool IsOpened { get; private set; }

        public void Reset()
        {
            IsOpened = false;
            _view.ResetDoor();
        }

        public void Open(float deltaTime)
        {
            IsOpened = true;
            _view.Open(deltaTime);
        }
    }
}
