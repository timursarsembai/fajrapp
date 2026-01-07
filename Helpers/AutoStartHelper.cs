using Microsoft.Win32;
using System;
using System.Reflection;

namespace FajrApp.Helpers;

public static class AutoStartHelper
{
    private const string AppName = "FajrApp";
    private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    
    public static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }
    
    public static void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
            if (key == null) return;
            
            if (enable)
            {
                var exePath = Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location;
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch
        {
            // Silently fail
        }
    }
}
