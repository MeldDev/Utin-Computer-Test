using System;
using UniRx;

namespace UtinComputerTest.UI.Windows
{
    public interface IWindowService
    {
        IObservable<BaseWindow> WindowOpened { get; }
        IObservable<BaseWindow> WindowClosed { get; }
        TWindow GetWindow<TWindow>() where TWindow : BaseWindow;
        TWindow OpenWindow<TWindow>() where TWindow : BaseWindow;
        void CloseWindow<TWindow>() where TWindow : BaseWindow;
        void CloseAllWindows();
        bool IsOpened<TWindow>() where TWindow : BaseWindow;
    }
}
