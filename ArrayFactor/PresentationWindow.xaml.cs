using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ArrayFactor
{
    public partial class PresentationWindow
    {
        public PresentationWindow() { InitializeComponent(); /*Loaded += OnLoaded;*/ }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            HwndSource.FromHwnd(new WindowInteropHelper(this).Handle).AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr Hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool Handled)
        {
            const uint WM_SYSTEMMENU = 0x121;
            const uint WP_SYSTEMMENU = 0x02;
            Debug.WriteLine($"m:{msg}(w:{wParam}; l:{lParam})");

            if(msg != WM_SYSTEMMENU || wParam.ToInt32() != WP_SYSTEMMENU) return IntPtr.Zero;
            ((ContextMenu)Resources["SystemMenu"]).IsOpen = true;
            Handled = true;
            return IntPtr.Zero;
        }
    }
}
