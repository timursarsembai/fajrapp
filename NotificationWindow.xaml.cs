using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FajrApp.Services;

namespace FajrApp;

public partial class NotificationWindow : Window
{
    private readonly DispatcherTimer _autoCloseTimer;
    
    public NotificationWindow(string prayerName, string prayerTime)
    {
        InitializeComponent();
        
        // Set content
        TitleText.Text = LocalizationService.T("PrayerTime");
        PrayerNameText.Text = prayerName;
        PrayerTimeText.Text = prayerTime;
        
        // Position in bottom-right corner
        Loaded += NotificationWindow_Loaded;
        
        // Allow dragging
        MouseLeftButtonDown += (s, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };
        
        // Auto-close after 10 seconds
        _autoCloseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        _autoCloseTimer.Tick += (s, e) =>
        {
            _autoCloseTimer.Stop();
            CloseWithAnimation();
        };
        _autoCloseTimer.Start();
    }
    
    private void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Position in bottom-right corner of the screen
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - ActualWidth - 20;
        Top = workArea.Bottom - ActualHeight - 20;
        
        // Fade in animation
        Opacity = 0;
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300)
        };
        BeginAnimation(OpacityProperty, fadeIn);
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _autoCloseTimer.Stop();
        
        // Stop any playing sound
        NotificationService.StopSound();
        
        CloseWithAnimation();
    }
    
    private void CloseWithAnimation()
    {
        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        fadeOut.Completed += (s, e) => Close();
        BeginAnimation(OpacityProperty, fadeOut);
    }
}
