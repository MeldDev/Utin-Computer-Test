using System;
using System.Collections.Generic;
using UniRx;

namespace UtinComputerTest.UI.Windows
{
    public sealed class WindowService : IWindowService, IDisposable
    {
        private readonly IWindowFactory _windowFactory;
        private readonly Dictionary<Type, BaseWindow> _windows = new();
        private readonly Subject<BaseWindow> _windowOpened = new();
        private readonly Subject<BaseWindow> _windowClosed = new();
        private readonly CompositeDisposable _disposables = new();

        public WindowService(IWindowFactory windowFactory)
        {
            _windowFactory = windowFactory;
        }

        public IObservable<BaseWindow> WindowOpened => _windowOpened;
        public IObservable<BaseWindow> WindowClosed => _windowClosed;

        public TWindow GetWindow<TWindow>() where TWindow : BaseWindow
        {
            var windowType = typeof(TWindow);
            if (_windows.TryGetValue(windowType, out var existingWindow))
            {
                return (TWindow)existingWindow;
            }

            var window = _windowFactory.CreateWindow<TWindow>();
            window.gameObject.SetActive(false);
            _windows.Add(windowType, window);

            window.Opened
                .Subscribe(_ => _windowOpened.OnNext(window))
                .AddTo(_disposables);

            window.Closed
                .Subscribe(_ => _windowClosed.OnNext(window))
                .AddTo(_disposables);

            return window;
        }

        public TWindow OpenWindow<TWindow>() where TWindow : BaseWindow
        {
            var window = GetWindow<TWindow>();
            window.Open();
            return window;
        }

        public void CloseWindow<TWindow>() where TWindow : BaseWindow
        {
            GetWindow<TWindow>().Close();
        }

        public void CloseAllWindows()
        {
            foreach (var window in _windows.Values)
            {
                window.Close();
            }
        }

        public bool IsOpened<TWindow>() where TWindow : BaseWindow
        {
            return _windows.TryGetValue(typeof(TWindow), out var window) && window.IsOpened;
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _windowOpened.Dispose();
            _windowClosed.Dispose();
        }
    }
}
