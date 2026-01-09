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
    
    // Notification tracking
    private string _lastNotifiedPrayer = "";
    private DateTime _lastNotifiedDate = DateTime.MinValue;
    
    // Data refresh tracking
    private DateTime _lastDataDate = DateTime.MinValue;
    private int _retryCount = 0;
    private const int MaxRetries = 3;
    
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
        
        // Apply theme and opacity
        ApplyTheme();
        
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
        Deactivated += MainWindow_Deactivated;
        
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
        
        // Subscribe to global mouse hook for closing popups
        MouseHook.MouseDown += OnGlobalMouseDown;
        MouseHook.Start();
        
        Closed += (s, e) => MouseHook.Stop();
    }
    
    private void OnGlobalMouseDown(Point screenPoint)
    {
        // Check if click is outside widget bounds
        var widgetRect = new Rect(Left, Top, ActualWidth, ActualHeight);
        
        // Check if menu popup is open and if click is inside it
        if (MenuPopup.IsOpen)
        {
            // Get popup position - it's placed above the widget
            var popupChild = MenuPopup.Child as FrameworkElement;
            if (popupChild != null && popupChild.IsVisible)
            {
                try
                {
                    // Get the popup's screen position
                    var popupPosition = popupChild.PointToScreen(new Point(0, 0));
                    var popupRect = new Rect(popupPosition.X, popupPosition.Y, 
                        popupChild.ActualWidth, popupChild.ActualHeight);
                    
                    // If click is inside popup OR inside widget, don't close
                    if (popupRect.Contains(screenPoint) || widgetRect.Contains(screenPoint))
                    {
                        return;
                    }
                }
                catch
                {
                    // Popup not ready yet, don't close
                    return;
                }
            }
            else
            {
                // Popup child not ready, don't close
                return;
            }
            
            // Click is outside popup and widget, close menu
            MenuPopup.IsOpen = false;
            return;
        }
        
        if (!widgetRect.Contains(screenPoint))
        {
            // Close prayer times window if open
            if (_prayerTimesWindow != null && _prayerTimesWindow.IsVisible)
            {
                // Check if click is inside prayer window
                var prayerRect = new Rect(_prayerTimesWindow.Left, _prayerTimesWindow.Top, 
                    _prayerTimesWindow.ActualWidth, _prayerTimesWindow.ActualHeight);
                
                if (!prayerRect.Contains(screenPoint))
                {
                    _prayerTimesWindow.Close();
                    _prayerTimesWindow = null;
                }
            }
        }
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
        MenuSupportProjectText.Text = LocalizationService.T("SupportProject");
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
    
    private void ApplyTheme()
    {
        // Apply theme colors
        System.Windows.Media.Color backgroundColor;
        System.Windows.Media.Color textColor;
        System.Windows.Media.Color menuBackgroundColor;
        System.Windows.Media.Color iconColor;
        
        if (_settings.Theme == AppTheme.Light)
        {
            // Light theme colors
            backgroundColor = System.Windows.Media.Color.FromArgb(230, 240, 240, 240);
            textColor = System.Windows.Media.Color.FromRgb(30, 30, 30);
            menuBackgroundColor = System.Windows.Media.Color.FromArgb(232, 245, 245, 245);
            iconColor = System.Windows.Media.Color.FromArgb(170, 30, 30, 30); // Semi-transparent dark
        }
        else
        {
            // Dark theme colors
            backgroundColor = System.Windows.Media.Color.FromArgb(204, 32, 32, 32);
            textColor = System.Windows.Media.Color.FromRgb(255, 255, 255);
            menuBackgroundColor = System.Windows.Media.Color.FromArgb(232, 32, 32, 32);
            iconColor = System.Windows.Media.Color.FromArgb(170, 255, 255, 255); // Semi-transparent white
        }
        
        // Apply opacity to background
        var opacity = (byte)(255 * _settings.WidgetOpacity);
        backgroundColor.A = opacity;
        
        // Update main border background
        MainBorder.Background = new System.Windows.Media.SolidColorBrush(backgroundColor);
        
        // Update menu popup background
        MenuBorder.Background = new System.Windows.Media.SolidColorBrush(menuBackgroundColor);
        
        // Update all text elements
        var textBrush = new System.Windows.Media.SolidColorBrush(textColor);
        NextPrayerName.Foreground = textBrush;
        NextPrayerTime.Foreground = textBrush;
        CountdownLabel.Foreground = textBrush;
        Countdown.Foreground = textBrush;
        
        // Update menu text colors
        MenuSettingsText.Foreground = textBrush;
        MenuChangePositionText.Foreground = textBrush;
        MenuAutoStartText.Foreground = textBrush;
        MenuSupportProjectText.Foreground = textBrush;
        MenuAboutText.Foreground = textBrush;
        MenuExitText.Foreground = textBrush;
        
        // Update menu icon colors
        var iconBrush = new System.Windows.Media.SolidColorBrush(iconColor);
        MenuSettingsIcon.Foreground = iconBrush;
        MenuChangePositionIcon.Foreground = iconBrush;
        MenuSupportProjectIcon.Foreground = iconBrush;
        MenuAboutIcon.Foreground = iconBrush;
        MenuExitIcon.Foreground = iconBrush;
    }
    
    private void MainWindow_Deactivated(object? sender, EventArgs e)
    {
        // Close popup when window loses focus
        if (MenuPopup.IsOpen)
        {
            MenuPopup.IsOpen = false;
        }
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
    
    // Prevent menu from closing when clicking inside it
    private void MenuBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
    
    private void MenuSettings_Click(object sender, MouseButtonEventArgs e)
    {
        MenuPopup.IsOpen = false;
        e.Handled = true;
        Dispatcher.BeginInvoke(() => OpenSettings());
    }
    
    private void MenuChangePosition_Click(object sender, MouseButtonEventArgs e)
    {
        MenuPopup.IsOpen = false;
        e.Handled = true;
        Dispatcher.BeginInvoke(() => EnterMoveMode());
    }
    
    private void MenuAutoStart_Click(object sender, MouseButtonEventArgs e)
    {
        var isEnabled = AutoStartHelper.IsAutoStartEnabled();
        AutoStartHelper.SetAutoStart(!isEnabled);
        _settings.AutoStart = !isEnabled;
        SettingsService.Save(_settings);
        UpdateAutoStartCheckbox();
        e.Handled = true;
    }
    
    private void MenuSupportProject_Click(object sender, MouseButtonEventArgs e)
    {
        MenuPopup.IsOpen = false;
        e.Handled = true;
        Dispatcher.BeginInvoke(() =>
        {
            var donateWindow = new DonateWindow();
            donateWindow.ShowDialog();
        });
    }
    
    private void MenuAbout_Click(object sender, MouseButtonEventArgs e)
    {
        MenuPopup.IsOpen = false;
        e.Handled = true;
        Dispatcher.BeginInvoke(() =>
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        });
    }
    
    private void MenuExit_Click(object sender, MouseButtonEventArgs e)
    {
        MenuPopup.IsOpen = false;
        e.Handled = true;
        _timer.Stop();
        MouseHook.Stop();
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
        
        // Setup periodic retry for failed data loads
        SetupPeriodicRetry();
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
        // Close menu if open
        if (MenuPopup.IsOpen)
        {
            MenuPopup.IsOpen = false;
            return;
        }
        
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
            _retryCount = 0; // Reset retry count on manual load
            _currentTimes = await _prayerService.GetPrayerTimesAsync(_settings);
            
            if (_currentTimes != null)
            {
                _lastDataDate = DateTime.Today;
                UpdateDisplay();
            }
            else
            {
                ShowError();
            }
        }
        catch (Exception ex)
        {
            NextPrayerName.Text = LocalizationService.T("Error");
            NextPrayerTime.Text = "";
            Countdown.Text = ex.Message.Length > 20 ? ex.Message[..20] + "..." : ex.Message;
        }
    }
    
    private void Timer_Tick(object? sender, EventArgs e)
    {
        // Check if data needs refresh (new day or no data)
        if (_lastDataDate.Date != DateTime.Today || _currentTimes == null)
        {
            _ = RefreshDataIfNeededAsync();
        }
        
        if (_currentTimes != null)
        {
            UpdateDisplay();
        }
    }
    
    private async System.Threading.Tasks.Task RefreshDataIfNeededAsync()
    {
        // Don't retry too often - but reset every 10 minutes to keep trying
        if (_retryCount >= MaxRetries)
        {
            // Will be reset by the periodic check
            return;
        }
        
        try
        {
            _currentTimes = await _prayerService.GetPrayerTimesAsync(_settings);
            
            if (_currentTimes != null)
            {
                _lastDataDate = DateTime.Today;
                _retryCount = 0;
                UpdateDisplay();
            }
            else
            {
                _retryCount++;
                ShowError();
            }
        }
        catch
        {
            _retryCount++;
            ShowError();
        }
    }
    
    private void ShowError()
    {
        NextPrayerName.Text = LocalizationService.T("Error");
        NextPrayerTime.Text = "";
        Countdown.Text = LocalizationService.T("NoData");
    }
    
    private void SetupPeriodicRetry()
    {
        // Reset retry count every 10 minutes to allow periodic retry attempts
        var retryTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(10)
        };
        retryTimer.Tick += (s, e) =>
        {
            if (_currentTimes == null || _lastDataDate.Date != DateTime.Today)
            {
                _retryCount = 0; // Allow new retry attempts
            }
        };
        retryTimer.Start();
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
        
        // Check for prayer time notification
        CheckAndNotifyPrayerTime(_currentTimes);
    }
    
    private void CheckAndNotifyPrayerTime(PrayerTimes times)
    {
        if (!_settings.NotificationsEnabled) return;
        
        var now = DateTime.Now;
        var today = DateTime.Today;
        
        // Reset notification tracking for new day
        if (_lastNotifiedDate.Date != today)
        {
            _lastNotifiedPrayer = "";
            _lastNotifiedDate = today;
        }
        
        // Check each prayer time
        var prayers = new[]
        {
            ("Fajr", times.Fajr, true),
            ("Sunrise", times.Sunrise, false),
            ("Dhuhr", times.Dhuhr, false),
            ("Asr", times.Asr, false),
            ("Maghrib", times.Maghrib, false),
            ("Isha", times.Isha, false)
        };
        
        foreach (var (prayerKey, prayerTime, isFajr) in prayers)
        {
            // Skip if already notified for this prayer today
            if (_lastNotifiedPrayer == prayerKey) continue;
            
            // Skip Sunrise notifications (it's not a prayer)
            if (prayerKey == "Sunrise") continue;
            
            // Check if we're within 30 seconds of prayer time
            var timeDiff = (now - prayerTime).TotalSeconds;
            if (timeDiff >= 0 && timeDiff < 30)
            {
                _lastNotifiedPrayer = prayerKey;
                _lastNotifiedDate = today;
                
                var localizedName = LocalizationService.T(prayerKey);
                var formattedTime = prayerTime.ToString("HH:mm");
                
                NotificationService.NotifyPrayerTime(
                    localizedName, 
                    formattedTime, 
                    _settings.NotificationSound,
                    isFajr);
                
                break;
            }
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
            ApplyTheme(); // Apply theme changes
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
