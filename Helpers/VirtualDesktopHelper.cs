using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace FajrApp.Helpers;

public static class VirtualDesktopHelper
{
    #region Win32 API
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;
    
    private const int SW_SHOWNOACTIVATE = 4;
    
    #endregion
    
    private static DispatcherTimer? _visibilityTimer;
    private static Window? _pinnedWindow;
    private static IntPtr _windowHandle;
    
    /// <summary>
    /// Makes the window appear on all virtual desktops and maintains visibility
    /// </summary>
    public static void PinToAllDesktops(Window window)
    {
        _pinnedWindow = window;
        
        try
        {
            var helper = new WindowInteropHelper(window);
            _windowHandle = helper.Handle;
            
            if (_windowHandle == IntPtr.Zero)
            {
                window.SourceInitialized += (s, e) =>
                {
                    PinToAllDesktops(window);
                };
                return;
            }
            
            // Set extended window styles for always-on-top behavior
            int exStyle = GetWindowLong(_windowHandle, GWL_EXSTYLE);
            exStyle |= WS_EX_TOOLWINDOW;    // Tool window - no taskbar button
            exStyle |= WS_EX_NOACTIVATE;    // Don't activate when shown
            exStyle &= ~WS_EX_APPWINDOW;    // Not an app window
            SetWindowLong(_windowHandle, GWL_EXSTYLE, exStyle);
            
            // Ensure topmost
            EnsureTopmost();
            
            // Start visibility monitor timer
            StartVisibilityMonitor();
        }
        catch
        {
            // Silently fail
        }
    }
    
    private static void EnsureTopmost()
    {
        if (_windowHandle != IntPtr.Zero)
        {
            SetWindowPos(_windowHandle, HWND_TOPMOST, 0, 0, 0, 0, 
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
    }
    
    private static void StartVisibilityMonitor()
    {
        if (_visibilityTimer != null) return;
        
        _visibilityTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        
        _visibilityTimer.Tick += (s, e) =>
        {
            if (_pinnedWindow == null || _windowHandle == IntPtr.Zero) return;
            
            try
            {
                // Check if window is visible
                if (!IsWindowVisible(_windowHandle))
                {
                    // Force show
                    ShowWindow(_windowHandle, SW_SHOWNOACTIVATE);
                }
                
                // Ensure always on top
                EnsureTopmost();
                
                // Ensure window is positioned correctly
                if (_pinnedWindow.Left < -500 || _pinnedWindow.Top < -500)
                {
                    TaskbarPositioner.PositionWindow(_pinnedWindow);
                }
            }
            catch
            {
                // Ignore errors
            }
        };
        
        _visibilityTimer.Start();
    }
    
    public static void StopMonitoring()
    {
        _visibilityTimer?.Stop();
        _visibilityTimer = null;
    }
}
