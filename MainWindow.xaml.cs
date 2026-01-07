using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FajrApp.Helpers;
using FajrApp.Models;
using FajrApp.Services;

namespace FajrApp;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private readonly PrayerService _prayerService;
    private PrayerTimes? _currentTimes;
    private AppSettings _settings;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _prayerService = new PrayerService();
        _settings = SettingsService.Load();
        
        // Setup timer for countdown updates
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        
        // Setup window events
        Loaded += MainWindow_Loaded;
        MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
        SizeChanged += MainWindow_SizeChanged;
        SourceInitialized += MainWindow_SourceInitialized;
        
        // Initialize autostart menu item
        AutoStartMenuItem.IsChecked = AutoStartHelper.IsAutoStartEnabled();
        
        // Apply autostart setting if needed
        if (_settings.AutoStart && !AutoStartHelper.IsAutoStartEnabled())
        {
            AutoStartHelper.SetAutoStart(true);
        }
    }
    
    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        // Pin window to all virtual desktops
        VirtualDesktopHelper.PinToAllDesktops(this);
    }
    
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Position window initially (off-screen to measure)
        Left = -1000;
        Top = -1000;
        
        // Load prayer times
        _ = LoadPrayerTimesAsync();
        
        // Position after content is loaded
        Dispatcher.BeginInvoke(new Action(() =>
        {
            TaskbarPositioner.PositionWindow(this);
        }), DispatcherPriority.Loaded);
        
        // Start timer
        _timer.Start();
        
        // Setup midnight refresh
        SetupMidnightRefresh();
    }
    
    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Reposition when size changes
        if (IsLoaded && Left > -500)
        {
            TaskbarPositioner.PositionWindow(this);
        }
    }
    
    private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Show prayer times window on click
        if (_currentTimes != null)
        {
            var prayerWindow = new PrayerTimesWindow(_currentTimes, _settings);
            prayerWindow.ShowDialog();
            
            // Check if settings were requested
            if (prayerWindow.SettingsRequested)
            {
                OpenSettings();
            }
        }
    }
    
    private async System.Threading.Tasks.Task LoadPrayerTimesAsync()
    {
        try
        {
            _currentTimes = await _prayerService.GetPrayerTimesAsync(_settings);
            
            if (_currentTimes != null)
            {
                UpdateDisplay();
            }
            else
            {
                NextPrayerName.Text = "Ошибка";
                NextPrayerTime.Text = "";
                Countdown.Text = "Нет данных";
            }
        }
        catch (Exception ex)
        {
            NextPrayerName.Text = "Ошибка";
            NextPrayerTime.Text = "";
            Countdown.Text = ex.Message.Length > 20 ? ex.Message[..20] + "..." : ex.Message;
        }
    }
    
    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_currentTimes != null)
        {
            UpdateDisplay();
        }
    }
    
    private void UpdateDisplay()
    {
        if (_currentTimes == null) return;
        
        var (name, time, remaining) = PrayerService.GetNextPrayer(_currentTimes);
        
        NextPrayerName.Text = name;
        NextPrayerTime.Text = time.ToString("HH:mm");
        
        // Format countdown
        if (remaining.TotalHours >= 1)
        {
            Countdown.Text = $"{(int)remaining.TotalHours}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
        else
        {
            Countdown.Text = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }
    
    private void SetupMidnightRefresh()
    {
        var now = DateTime.Now;
        var midnight = now.Date.AddDays(1);
        var timeUntilMidnight = midnight - now;
        
        var midnightTimer = new DispatcherTimer
        {
            Interval = timeUntilMidnight
        };
        
        midnightTimer.Tick += async (s, e) =>
        {
            midnightTimer.Stop();
            await LoadPrayerTimesAsync();
            SetupMidnightRefresh(); // Setup for next day
        };
        
        midnightTimer.Start();
    }
    
    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        OpenSettings();
    }
    
    private void OpenSettings()
    {
        var settingsWindow = new SettingsWindow(_settings);
        
        if (settingsWindow.ShowDialog() == true)
        {
            _settings = SettingsService.Load();
            _ = LoadPrayerTimesAsync();
        }
    }
    
    private void AutoStart_Click(object sender, RoutedEventArgs e)
    {
        var isEnabled = AutoStartMenuItem.IsChecked;
        AutoStartHelper.SetAutoStart(isEnabled);
        
        _settings.AutoStart = isEnabled;
        SettingsService.Save(_settings);
    }
    
    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        // Clear cache to force refresh
        _settings.CachedTimes = null;
        SettingsService.Save(_settings);
        
        await LoadPrayerTimesAsync();
    }
    
    private void About_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.ShowDialog();
    }
    
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        VirtualDesktopHelper.StopMonitoring();
        Application.Current.Shutdown();
    }
}
