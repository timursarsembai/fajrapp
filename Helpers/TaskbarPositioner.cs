using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FajrApp.Helpers;

public static class TaskbarPositioner
{
    #region Win32 API Declarations
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);
    
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    
    [DllImport("shell32.dll")]
    private static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);
    
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
    
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public int lParam;
    }
    
    private const uint ABM_GETTASKBARPOS = 0x00000005;
    private const uint ABE_LEFT = 0;
    private const uint ABE_TOP = 1;
    private const uint ABE_RIGHT = 2;
    private const uint ABE_BOTTOM = 3;
    
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;
    
    #endregion
    
    public enum TaskbarPosition
    {
        Bottom,
        Top,
        Left,
        Right,
        Unknown
    }
    
    public static TaskbarPosition GetTaskbarPosition()
    {
        var data = new APPBARDATA { cbSize = (uint)Marshal.SizeOf<APPBARDATA>() };
        
        if (SHAppBarMessage(ABM_GETTASKBARPOS, ref data) != 0)
        {
            return data.uEdge switch
            {
                ABE_LEFT => TaskbarPosition.Left,
                ABE_TOP => TaskbarPosition.Top,
                ABE_RIGHT => TaskbarPosition.Right,
                ABE_BOTTOM => TaskbarPosition.Bottom,
                _ => TaskbarPosition.Unknown
            };
        }
        
        return TaskbarPosition.Bottom;
    }
    
    public static (int Left, int Top, int Right, int Bottom) GetTaskbarRect()
    {
        IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", null);
        
        if (taskbarHandle != IntPtr.Zero && GetWindowRect(taskbarHandle, out RECT rect))
        {
            return (rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
        
        // Fallback: assume bottom taskbar
        int screenHeight = GetSystemMetrics(SM_CYSCREEN);
        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        return (0, screenHeight - 48, screenWidth, screenHeight);
    }
    
    public static (int Left, int Top, int Right, int Bottom)? GetTrayNotifyRect()
    {
        IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", null);
        if (taskbarHandle == IntPtr.Zero) return null;
        
        IntPtr trayNotify = FindWindowEx(taskbarHandle, IntPtr.Zero, "TrayNotifyWnd", null);
        if (trayNotify == IntPtr.Zero)
        {
            // Windows 11 style
            trayNotify = FindWindowEx(taskbarHandle, IntPtr.Zero, "Windows.UI.Composition.DesktopWindowContentBridge", null);
        }
        
        if (trayNotify != IntPtr.Zero && GetWindowRect(trayNotify, out RECT rect))
        {
            return (rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
        
        return null;
    }
    
    public static void PositionWindow(Window window)
    {
        var taskbarRect = GetTaskbarRect();
        var trayRect = GetTrayNotifyRect();
        var position = GetTaskbarPosition();
        
        // Get DPI scaling
        var source = PresentationSource.FromVisual(window);
        double dpiX = 1.0, dpiY = 1.0;
        if (source?.CompositionTarget != null)
        {
            dpiX = source.CompositionTarget.TransformToDevice.M11;
            dpiY = source.CompositionTarget.TransformToDevice.M22;
        }
        
        double windowWidth = window.ActualWidth;
        double windowHeight = window.ActualHeight;
        
        // If window not yet measured, use default
        if (windowWidth <= 0) windowWidth = 200;
        if (windowHeight <= 0) windowHeight = 40;
        
        switch (position)
        {
            case TaskbarPosition.Bottom:
            {
                int taskbarHeight = taskbarRect.Bottom - taskbarRect.Top;
                double y = (taskbarRect.Top + (taskbarHeight - windowHeight * dpiY) / 2) / dpiY;
                
                double x;
                if (trayRect.HasValue)
                {
                    // Position to the left of system tray with small gap
                    x = (trayRect.Value.Left - windowWidth * dpiX - 8) / dpiX;
                }
                else
                {
                    // Fallback: position near right edge
                    x = (taskbarRect.Right - windowWidth * dpiX - 150) / dpiX;
                }
                
                window.Left = x;
                window.Top = y;
                break;
            }
            
            case TaskbarPosition.Top:
            {
                int taskbarHeight = taskbarRect.Bottom - taskbarRect.Top;
                double y = (taskbarRect.Top + (taskbarHeight - windowHeight * dpiY) / 2) / dpiY;
                
                double x;
                if (trayRect.HasValue)
                {
                    x = (trayRect.Value.Left - windowWidth * dpiX - 8) / dpiX;
                }
                else
                {
                    x = (taskbarRect.Right - windowWidth * dpiX - 150) / dpiX;
                }
                
                window.Left = x;
                window.Top = y;
                break;
            }
            
            case TaskbarPosition.Right:
            {
                int taskbarWidth = taskbarRect.Right - taskbarRect.Left;
                double x = (taskbarRect.Left + (taskbarWidth - windowWidth * dpiX) / 2) / dpiX;
                
                double y;
                if (trayRect.HasValue)
                {
                    y = (trayRect.Value.Top - windowHeight * dpiY - 8) / dpiY;
                }
                else
                {
                    y = (taskbarRect.Bottom - windowHeight * dpiY - 150) / dpiY;
                }
                
                window.Left = x;
                window.Top = y;
                break;
            }
            
            case TaskbarPosition.Left:
            {
                int taskbarWidth = taskbarRect.Right - taskbarRect.Left;
                double x = (taskbarRect.Left + (taskbarWidth - windowWidth * dpiX) / 2) / dpiX;
                
                double y;
                if (trayRect.HasValue)
                {
                    y = (trayRect.Value.Top - windowHeight * dpiY - 8) / dpiY;
                }
                else
                {
                    y = (taskbarRect.Bottom - windowHeight * dpiY - 150) / dpiY;
                }
                
                window.Left = x;
                window.Top = y;
                break;
            }
            
            default:
                // Unknown position, place in bottom right corner
                window.Left = SystemParameters.PrimaryScreenWidth - windowWidth - 150;
                window.Top = SystemParameters.PrimaryScreenHeight - windowHeight - 60;
                break;
        }
    }
}
