using System;
using UniRx;
using UnityEngine;

namespace UtinComputerTest.UI.Windows
{
    public abstract class BaseWindow : MonoBehaviour
    {
        [SerializeField] private WindowLayer _layer = WindowLayer.Normal;
        [SerializeField] private bool _blocksInput = true;
        [SerializeField] private bool _canCloseByBackdropClick;

        private readonly Subject<Unit> _opened = new();
        private readonly Subject<Unit> _closed = new();

        public WindowLayer Layer => _layer;
        public bool BlocksInput => _blocksInput;
        public bool CanCloseByBackdropClick => _canCloseByBackdropClick;
        public bool IsOpened { get; private set; }
        public IObservable<Unit> Opened => _opened;
        public IObservable<Unit> Closed => _closed;

        protected virtual void Awake()
        {
            if (_layer == 0)
            {
                _layer = WindowLayer.Normal;
            }
        }

        public void Open()
        {
            if (IsOpened)
            {
                return;
            }

            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            IsOpened = true;
            OnOpened();
            _opened.OnNext(Unit.Default);
        }

        public virtual void Close()
        {
            if (!IsOpened)
            {
                return;
            }

            IsOpened = false;
            OnClosed();
            gameObject.SetActive(false);
            _closed.OnNext(Unit.Default);
        }

        protected virtual void OnOpened()
        {
        }

        protected virtual void OnClosed()
        {
        }
    }
}
