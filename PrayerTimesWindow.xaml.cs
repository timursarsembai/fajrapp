using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using FajrApp.Helpers;
using FajrApp.Models;
using FajrApp.Services;

namespace FajrApp;

public partial class PrayerTimesWindow : Window
{
    private readonly PrayerTimes _times;
    private readonly AppSettings _settings;
    private readonly Rect _widgetRect;
    
    public bool SettingsRequested { get; private set; }
    
    public PrayerTimesWindow(PrayerTimes times, AppSettings settings, Rect widgetRect)
    {
        InitializeComponent();
        
        _times = times;
        _settings = settings;
        _widgetRect = widgetRect;
        
        // Allow dragging the window
        MouseLeftButtonDown += (s, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };
        
        // Close on Escape
        KeyDown += (s, e) =>
        {
            if (e.Key == Key.Escape)
                SafeClose();
        };
        
        // Close when clicked outside (deactivated)
        Deactivated += (s, e) =>
        {
            if (!SettingsRequested)
                SafeClose();
        };
        
        Loaded += PrayerTimesWindow_Loaded;
        
        LoadTimes();
    }
    
    private void PrayerTimesWindow_Loaded(object sender, RoutedEventArgs e)
    {
        PositionNearTaskbar();
        PlaySlideUpAnimation();
    }
    
    private void PositionNearTaskbar()
    {
        var taskbarRect = TaskbarPositioner.GetTaskbarRect();
        var position = TaskbarPositioner.GetTaskbarPosition();
        
        double windowWidth = ActualWidth;
        double windowHeight = ActualHeight;
        
        // Position relative to widget
        switch (position)
        {
            case TaskbarPositioner.TaskbarPosition.Bottom:
            {
                // Position above widget, centered on it
                double x = _widgetRect.Left + (_widgetRect.Width / 2) - (windowWidth / 2);
                double y = _widgetRect.Top - windowHeight - 10;
                
                // Keep on screen
                x = Math.Max(10, Math.Min(x, SystemParameters.PrimaryScreenWidth - windowWidth - 10));
                
                Left = x;
                Top = y;
                break;
            }
            
            case TaskbarPositioner.TaskbarPosition.Top:
            {
                // Position below widget, centered on it
                double x = _widgetRect.Left + (_widgetRect.Width / 2) - (windowWidth / 2);
                double y = _widgetRect.Bottom + 10;
                
                x = Math.Max(10, Math.Min(x, SystemParameters.PrimaryScreenWidth - windowWidth - 10));
                
                Left = x;
                Top = y;
                break;
            }
            
            case TaskbarPositioner.TaskbarPosition.Right:
            {
                // Position to the left of widget
                double x = _widgetRect.Left - windowWidth - 10;
                double y = _widgetRect.Top + (_widgetRect.Height / 2) - (windowHeight / 2);
                
                y = Math.Max(10, Math.Min(y, SystemParameters.PrimaryScreenHeight - windowHeight - 10));
                
                Left = x;
                Top = y;
                break;
            }
            
            case TaskbarPositioner.TaskbarPosition.Left:
            {
                // Position to the right of widget
                double x = _widgetRect.Right + 10;
                double y = _widgetRect.Top + (_widgetRect.Height / 2) - (windowHeight / 2);
                
                y = Math.Max(10, Math.Min(y, SystemParameters.PrimaryScreenHeight - windowHeight - 10));
                
                Left = x;
                Top = y;
                break;
            }
            
            default:
                // Center on screen
                Left = (SystemParameters.PrimaryScreenWidth - windowWidth) / 2;
                Top = (SystemParameters.PrimaryScreenHeight - windowHeight) / 2;
                break;
        }
    }
    
    private void PlaySlideUpAnimation()
    {
        var position = TaskbarPositioner.GetTaskbarPosition();
        
        // Initial offset
        double startOffset = 50;
        var translateTransform = new System.Windows.Media.TranslateTransform();
        RenderTransform = translateTransform;
        
        // Determine animation direction based on taskbar position
        DependencyProperty animatedProperty;
        double from, to;
        
        switch (position)
        {
            case TaskbarPositioner.TaskbarPosition.Bottom:
                animatedProperty = System.Windows.Media.TranslateTransform.YProperty;
                from = startOffset;
                to = 0;
                break;
            case TaskbarPositioner.TaskbarPosition.Top:
                animatedProperty = System.Windows.Media.TranslateTransform.YProperty;
                from = -startOffset;
                to = 0;
                break;
            case TaskbarPositioner.TaskbarPosition.Right:
                animatedProperty = System.Windows.Media.TranslateTransform.XProperty;
                from = startOffset;
                to = 0;
                break;
            case TaskbarPositioner.TaskbarPosition.Left:
                animatedProperty = System.Windows.Media.TranslateTransform.XProperty;
                from = -startOffset;
                to = 0;
                break;
            default:
                return;
        }
        
        // Create animation
        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        // Fade in animation
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        
        BeginAnimation(OpacityProperty, fadeAnimation);
        translateTransform.BeginAnimation(animatedProperty, animation);
    }
    
    private void LoadTimes()
    {
        FajrTime.Text = _times.Fajr.ToString("HH:mm");
        SunriseTime.Text = _times.Sunrise.ToString("HH:mm");
        DhuhrTime.Text = _times.Dhuhr.ToString("HH:mm");
        AsrTime.Text = _times.Asr.ToString("HH:mm");
        MaghribTime.Text = _times.Maghrib.ToString("HH:mm");
        IshaTime.Text = _times.Isha.ToString("HH:mm");
        
        CityText.Text = _settings.City;
        
        // Highlight current prayer
        var currentPrayer = PrayerService.GetCurrentPrayerName(_times);
        
        if (currentPrayer == "Фаджр") FajrLabel.FontWeight = FontWeights.Bold;
        else if (currentPrayer == "Восход") SunriseLabel.FontWeight = FontWeights.Bold;
        else if (currentPrayer == "Зухр") DhuhrLabel.FontWeight = FontWeights.Bold;
        else if (currentPrayer == "Аср") AsrLabel.FontWeight = FontWeights.Bold;
        else if (currentPrayer == "Магриб") MaghribLabel.FontWeight = FontWeights.Bold;
        else if (currentPrayer == "Иша") IshaLabel.FontWeight = FontWeights.Bold;
    }
    
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsRequested = true;
        SafeClose();
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        SafeClose();
    }
    
    private bool _isClosing = false;
    
    private void SafeClose()
    {
        if (_isClosing) return;
        _isClosing = true;
        
        // Use Dispatcher to close safely, avoiding the "Cannot set Visibility" error
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
}
