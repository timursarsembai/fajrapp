using System;
using System.Threading;
using System.Windows;

namespace FajrApp;

public partial class App : Application
{
    private static Mutex? _mutex;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        const string mutexName = "FajrApp_SingleInstance_Mutex";
        
        _mutex = new Mutex(true, mutexName, out bool createdNew);
        
        if (!createdNew)
        {
            // Another instance is already running
            MessageBox.Show("FajrApp уже запущен.", "FajrApp", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            Current.Shutdown();
            return;
        }
        
        // Handle unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            MessageBox.Show($"Критическая ошибка: {ex?.Message}", "FajrApp - Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        };
        
        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show($"Ошибка: {args.Exception.Message}", "FajrApp - Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
        
        base.OnStartup(e);
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
