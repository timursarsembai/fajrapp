using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace FajrApp.Helpers;

public static class MouseHook
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    
    private static IntPtr _hookId = IntPtr.Zero;
    private static LowLevelMouseProc? _proc;
    
    public delegate void MouseDownHandler(Point screenPoint);
    public static event MouseDownHandler? MouseDown;
    
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
    
    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    
    public static void Start()
    {
        if (_hookId != IntPtr.Zero) return;
        
        _proc = HookCallback;
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        
        if (curModule != null)
        {
            _hookId = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }
    
    public static void Stop()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
        _proc = null;
    }
    
    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN))
        {
            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            var point = new Point(hookStruct.pt.x, hookStruct.pt.y);
            
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                MouseDown?.Invoke(point);
            });
        }
        
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }
}
