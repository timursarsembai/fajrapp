using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace FajrApp;

public partial class AboutWindow : Window
{
    public const string AppVersion = "1.0.0";
    public const string UpdateDate = "7 января 2026";
    public const string GitHubUrl = "https://github.com/timursarsembai/fajrapp";
    
    private bool _isClosing = false;
    
    public AboutWindow()
    {
        InitializeComponent();
        
        VersionText.Text = $"Версия {AppVersion}";
        UpdateDateText.Text = $"Обновлено: {UpdateDate}";
        
        // Close on Escape
        KeyDown += (s, e) =>
        {
            if (e.Key == Key.Escape)
                SafeClose();
        };
        
        // Allow dragging
        MouseLeftButtonDown += (s, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };
        
        // Close when deactivated
        Deactivated += (s, e) => SafeClose();
    }
    
    private void SafeClose()
    {
        if (_isClosing) return;
        _isClosing = true;
        
        Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                Close();
            }
            catch
            {
                // Ignore close errors
            }
        }), System.Windows.Threading.DispatcherPriority.Background);
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        SafeClose();
    }
    
    private void GitHubButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = GitHubUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            // Failed to open browser
        }
    }
}
