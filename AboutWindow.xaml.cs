using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using FajrApp.Services;

namespace FajrApp;

public partial class AboutWindow : Window
{
    public const string AppVersion = "1.0.0";
    public const string UpdateDate = "January 7, 2026";
    public const string GitHubUrl = "https://github.com/timursarsembai/fajrapp";
    
    private bool _isClosing = false;
    
    public AboutWindow()
    {
        InitializeComponent();
        
        UpdateLocalization();
        
        // Apply theme after window is loaded
        var settings = Services.SettingsService.Load();
        Loaded += (s, e) => Helpers.ThemeManager.ApplyTheme(this, settings);
        
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
    
    private void UpdateLocalization()
    {
        Title = LocalizationService.T("AboutTitle");
        HeaderText.Text = LocalizationService.T("AboutTitle");
        AppDescription.Text = LocalizationService.T("PrayerTimes") + " Widget for Windows";
        VersionText.Text = $"{LocalizationService.T("Version")} {AppVersion}";
        UpdateDateText.Text = $"{LocalizationService.T("Updated")}: {UpdateDate}";
        GitHubButton.Content = $"🔗 {LocalizationService.T("GitHub")}: timursarsembai/fajrapp";
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
