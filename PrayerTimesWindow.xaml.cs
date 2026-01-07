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
    
    public bool SettingsRequested { get; private set; }
    
    public PrayerTimesWindow(PrayerTimes times, AppSettings settings)
    {
        InitializeComponent();
        
        _times = times;
        _settings = settings;
        
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
                Close();
        };
        
        // Close when clicked outside (deactivated)
        Deactivated += (s, e) =>
        {
            if (!SettingsRequested)
                Close();
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
        var trayRect = TaskbarPositioner.GetTrayNotifyRect();
        var position = TaskbarPositioner.GetTaskbarPosition();
        
        double windowWidth = ActualWidth;
        double windowHeight = ActualHeight;
        
        switch (position)
        {
            case TaskbarPositioner.TaskbarPosition.Bottom:
            {
                // Position above taskbar, near the tray area
                double x;
                if (trayRect.HasValue)
                {
                    // Align right edge with tray area
                    x = trayRect.Value.Right - windowWidth - 10;
                }
                else
                {
                    x = SystemParameters.PrimaryScreenWidth - windowWidth - 20;
                }
                
                double y = taskbarRect.Top - windowHeight - 10;
                
                Left = Math.Max(10, x);
                Top = y;
                break;
            }
            
            case TaskbarPositioner.TaskbarPosition.Top:
            {
                double x;
                if (trayRect.HasValue)
                {
                    x = trayRect.Value.Right - windowWidth - 10;
                }
                else
                {
                    x = SystemParameters.PrimaryScreenWidth - windowWidth - 20;
                }
                
                double y = taskbarRect.Bottom + 10;
                
                Left = Math.Max(10, x);
                Top = y;
                break;
            }
            
            case TaskbarPositioner.TaskbarPosition.Right:
            {
                double x = taskbarRect.Left - windowWidth - 10;
                double y = SystemParameters.PrimaryScreenHeight - windowHeight - 60;
                
                Left = x;
                Top = Math.Max(10, y);
                break;
            }
            
            case TaskbarPositioner.TaskbarPosition.Left:
            {
                double x = taskbarRect.Right + 10;
                double y = SystemParameters.PrimaryScreenHeight - windowHeight - 60;
                
                Left = x;
                Top = Math.Max(10, y);
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
        Close();
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
