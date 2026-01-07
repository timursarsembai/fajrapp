using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
    private PrayerTimesWindow? _prayerTimesWindow;
    
    // Move mode state
    private bool _isInMoveMode;
    private bool _isDragging;
    private Point _dragStartPoint;
    private double _dragStartLeft;
    
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
        MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
        MouseMove += MainWindow_MouseMove;
        MouseRightButtonUp += MainWindow_MouseRightButtonUp;
        SizeChanged += MainWindow_SizeChanged;
        SourceInitialized += MainWindow_SourceInitialized;
        KeyDown += MainWindow_KeyDown;
        
        // Initialize autostart state
        UpdateAutoStartCheckbox();
        
        // Apply autostart setting if needed
        if (_settings.AutoStart && !AutoStartHelper.IsAutoStartEnabled())
        {
            AutoStartHelper.SetAutoStart(true);
        }
        
        // Subscribe to language changes
        LocalizationService.LanguageChanged += UpdateLocalization;
        UpdateLocalization();
    }
    
    private void UpdateLocalization()
    {
        // Update countdown label
        CountdownLabel.Text = LocalizationService.T("In");
        
        // Update move tooltip
        MoveTooltipText.Text = LocalizationService.T("DragWidget");
        
        // Update menu items
        MenuSettingsText.Text = LocalizationService.T("Settings");
        MenuChangePositionText.Text = LocalizationService.T("ChangePosition");
        MenuAutoStartText.Text = LocalizationService.T("AutoStart");
        MenuAboutText.Text = LocalizationService.T("About");
        MenuExitText.Text = LocalizationService.T("Exit");
    }
    
    private void UpdateAutoStartCheckbox()
    {
        var isEnabled = AutoStartHelper.IsAutoStartEnabled();
        AutoStartCheck.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
        AutoStartCheckBox.Background = isEnabled 
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 76, 175, 80))
            : System.Windows.Media.Brushes.Transparent;
    }
    
    private void MainWindow_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Toggle popup menu
        if (MenuPopup.IsOpen)
        {
            MenuPopup.IsOpen = false;
        }
        else
        {
            UpdateAutoStartCheckbox();
            MenuPopup.IsOpen = true;
        }
        e.Handled = true;
    }
    
    private void MenuSettings_Click(object sender, MouseButtonEventArgs e)
    {
        MenuPopup.IsOpen = false;
        OpenSettings();
    }
    
    private void MenuChangePosition_Click(object sender, MouseButtonEventArgs e)
    {
        MenuPopup.IsOpen = false;
        EnterMoveMode();
    }
    
    private void MenuAutoStart_Click(object sender, MouseButtonEventArgs e)
    {
        var isEnabled = AutoStartHelper.IsAutoStartEnabled();
        AutoStartHelper.SetAutoStart(!isEnabled);
        _settings.AutoStart = !isEnabled;
        SettingsService.Save(_settings);
        UpdateAutoStartCheckbox();
    }
    
    private void MenuAbout_Click(object sender, MouseButtonEventArgs e)
    {
        MenuPopup.IsOpen = false;
        var aboutWindow = new AboutWindow();
        aboutWindow.ShowDialog();
    }
    
    private void MenuExit_Click(object sender, MouseButtonEventArgs e)
    {
        MenuPopup.IsOpen = false;
        _timer.Stop();
        VirtualDesktopHelper.StopMonitoring();
        Application.Current.Shutdown();
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
        
        // Position after content is loaded with saved offset
        Dispatcher.BeginInvoke(new Action(() =>
        {
            TaskbarPositioner.PositionWindow(this, _settings.WidgetOffsetX);
        }), DispatcherPriority.Loaded);
        
        // Start timer
        _timer.Start();
        
        // Setup midnight refresh
        SetupMidnightRefresh();
    }
    
    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Reposition when size changes (unless in move mode)
        if (IsLoaded && Left > -500 && !_isInMoveMode)
        {
            TaskbarPositioner.PositionWindow(this, _settings.WidgetOffsetX);
        }
    }
    
    private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // In move mode - start dragging or finish move mode
        if (_isInMoveMode)
        {
            if (!_isDragging)
            {
                // Start dragging - use screen coordinates to avoid jitter
                _isDragging = true;
                _dragStartPoint = PointToScreen(e.GetPosition(this));
                _dragStartLeft = Left;
                CaptureMouse();
                Cursor = Cursors.SizeWE;
            }
            return;
        }
        
        // Normal mode - toggle prayer times window on click
        if (_prayerTimesWindow != null && _prayerTimesWindow.IsVisible)
        {
            // Close existing window
            _prayerTimesWindow.Close();
            _prayerTimesWindow = null;
            return;
        }
        
        if (_currentTimes != null)
        {
            // Get widget position to position popup above it
            var widgetRect = new Rect(Left, Top, ActualWidth, ActualHeight);
            
            var prayerWindow = new PrayerTimesWindow(_currentTimes, _settings, widgetRect);
            _prayerTimesWindow = prayerWindow;
            
            prayerWindow.Closed += (s, args) =>
            {
                _prayerTimesWindow = null;
                
                // Check if settings were requested
                if (prayerWindow.SettingsRequested)
                {
                    OpenSettings();
                }
            };
            
            prayerWindow.Show();
        }
    }
    
    private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            
            // Calculate and save offset
            CalculateAndSaveOffset();
            
            // Exit move mode
            ExitMoveMode();
        }
    }
    
    private void MainWindow_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && _isInMoveMode)
        {
            // Use screen coordinates to avoid jitter
            var currentScreenPos = PointToScreen(e.GetPosition(this));
            var deltaX = currentScreenPos.X - _dragStartPoint.X;
            
            // Move widget horizontally along taskbar
            var newLeft = _dragStartLeft + deltaX;
            
            // Constrain to screen bounds
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            newLeft = Math.Max(10, Math.Min(newLeft, screenWidth - ActualWidth - 10));
            
            Left = newLeft;
            
            // Update drag start for smooth continuous dragging
            _dragStartPoint = currentScreenPos;
            _dragStartLeft = Left;
        }
    }
    
    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        // Escape to cancel move mode
        if (e.Key == Key.Escape && _isInMoveMode)
        {
            _isDragging = false;
            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            
            // Reset to original position
            TaskbarPositioner.PositionWindow(this, _settings.WidgetOffsetX);
            
            ExitMoveMode();
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
    
    private void OpenSettings()
    {
        var settingsWindow = new SettingsWindow(_settings);
        
        if (settingsWindow.ShowDialog() == true)
        {
            _settings = SettingsService.Load();
            _ = LoadPrayerTimesAsync();
        }
    }
    
    private void EnterMoveMode()
    {
        _isInMoveMode = true;
        Cursor = Cursors.SizeWE;
        
        // Show visual feedback
        MoveTooltip.Visibility = Visibility.Visible;
        
        // Start pulse animation
        var pulseAnimation = (Storyboard)FindResource("PulseAnimation");
        pulseAnimation.Begin();
    }
    
    private void ExitMoveMode()
    {
        _isInMoveMode = false;
        _isDragging = false;
        Cursor = Cursors.Arrow;
        
        // Hide visual feedback
        MoveTooltip.Visibility = Visibility.Collapsed;
        MoveModeGlow.Opacity = 0;
        
        // Stop pulse animation
        var pulseAnimation = (Storyboard)FindResource("PulseAnimation");
        pulseAnimation.Stop();
        
        // Play landing animation
        var landingAnimation = (Storyboard)FindResource("LandingAnimation");
        landingAnimation.Begin();
    }
    
    private void CalculateAndSaveOffset()
    {
        // Get the default position without offset
        var taskbarRect = TaskbarPositioner.GetTaskbarRect();
        var trayRect = TaskbarPositioner.GetTrayNotifyRect();
        
        // Get DPI
        var source = PresentationSource.FromVisual(this);
        double dpiX = 1.0;
        if (source?.CompositionTarget != null)
        {
            dpiX = source.CompositionTarget.TransformToDevice.M11;
        }
        
        double defaultX;
        if (trayRect.HasValue)
        {
            defaultX = (trayRect.Value.Left - ActualWidth * dpiX - 8) / dpiX;
        }
        else
        {
            defaultX = (taskbarRect.Right - ActualWidth * dpiX - 150) / dpiX;
        }
        
        // Calculate offset from default position
        _settings.WidgetOffsetX = Left - defaultX;
        SettingsService.Save(_settings);
    }
    
}
