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
    
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    
    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
    
    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
    
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }
    
    private const uint MONITOR_DEFAULTTONEAREST = 2;
    
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
    private static bool _wasHiddenForFullscreen = false;
    
    /// <summary>
    /// Checks if there's a fullscreen application running in the foreground
    /// </summary>
    public static bool IsFullscreenAppRunning()
    {
        try
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero) return false;
            
            // Don't hide if our own window is in foreground
            if (foregroundWindow == _windowHandle) return false;
            
            // Get the monitor where the foreground window is
            IntPtr monitor = MonitorFromWindow(foregroundWindow, MONITOR_DEFAULTTONEAREST);
            if (monitor == IntPtr.Zero) return false;
            
            MONITORINFO monitorInfo = new MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            if (!GetMonitorInfo(monitor, ref monitorInfo)) return false;
            
            // Get window rect
            if (!GetWindowRect(foregroundWindow, out RECT windowRect)) return false;
            
            // Check if window covers the entire monitor (fullscreen)
            bool isFullscreen = 
                windowRect.Left <= monitorInfo.rcMonitor.Left &&
                windowRect.Top <= monitorInfo.rcMonitor.Top &&
                windowRect.Right >= monitorInfo.rcMonitor.Right &&
                windowRect.Bottom >= monitorInfo.rcMonitor.Bottom;
            
            return isFullscreen;
        }
        catch
        {
            return false;
        }
    }
    
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
                // Check if fullscreen app is running
                bool isFullscreen = IsFullscreenAppRunning();
                
                if (isFullscreen)
                {
                    // Hide widget when fullscreen app is active
                    if (_pinnedWindow.Visibility == Visibility.Visible)
                    {
                        _pinnedWindow.Visibility = Visibility.Hidden;
                        _wasHiddenForFullscreen = true;
                    }
                    return;
                }
                else if (_wasHiddenForFullscreen)
                {
                    // Restore widget when fullscreen app closes
                    _pinnedWindow.Visibility = Visibility.Visible;
                    _wasHiddenForFullscreen = false;
                }
                
                // Check if window is visible
                if (!IsWindowVisible(_windowHandle) && !_wasHiddenForFullscreen)
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
