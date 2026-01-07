using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using FajrApp.Helpers;
using FajrApp.Services;

namespace FajrApp;

public partial class MenuWindow : Window
{
    public bool SettingsRequested { get; private set; }
    public bool ChangePositionRequested { get; private set; }
    public bool AutoStartToggled { get; private set; }
    public bool AboutRequested { get; private set; }
    public bool ExitRequested { get; private set; }
    
    private bool _isAutoStartEnabled;
    private Rect _widgetRect;
    private bool _isClosing;
    
    public MenuWindow(bool isAutoStartEnabled, Rect widgetRect)
    {
        InitializeComponent();
        
        _isAutoStartEnabled = isAutoStartEnabled;
        _widgetRect = widgetRect;
        
        // Update auto start checkbox
        UpdateAutoStartCheckbox();
        
        // Apply localization
        UpdateLocalization();
        
        Loaded += MenuWindow_Loaded;
    }
    
    private void UpdateLocalization()
    {
        SettingsText.Text = LocalizationService.T("Settings");
        ChangePositionText.Text = LocalizationService.T("ChangePosition");
        AutoStartText.Text = LocalizationService.T("AutoStart");
        AboutText.Text = LocalizationService.T("About");
        ExitText.Text = LocalizationService.T("Exit");
    }
    
    private void UpdateAutoStartCheckbox()
    {
        AutoStartCheck.Visibility = _isAutoStartEnabled ? Visibility.Visible : Visibility.Collapsed;
        AutoStartCheckBox.Background = _isAutoStartEnabled 
            ? (System.Windows.Media.Brush)new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 76, 175, 80))
            : System.Windows.Media.Brushes.Transparent;
    }
    
    private void MenuWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Pin to all virtual desktops
        VirtualDesktopHelper.PinToAllDesktops(this);
        
        // Position menu now that we know its size
        PositionAboveWidget();
        
        // Start fade in animation
        var animation = (Storyboard)FindResource("FadeInAnimation");
        animation.Begin();
    }
    
    private void PositionAboveWidget()
    {
        // Now we have actual size
        var menuWidth = ActualWidth;
        var menuHeight = ActualHeight;
        
        // Position above widget, centered horizontally
        var left = _widgetRect.Left + (_widgetRect.Width / 2) - (menuWidth / 2);
        var top = _widgetRect.Top - menuHeight - 8; // 8px gap above widget
        
        // Ensure visible on screen
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        
        if (left < 8) left = 8;
        if (left + menuWidth > screenWidth - 8) left = screenWidth - menuWidth - 8;
        if (top < 8) top = _widgetRect.Bottom + 8; // Show below if no space above
        
        Left = left;
        Top = top;
    }
    
    private void Window_Deactivated(object sender, EventArgs e)
    {
        // Close when clicking elsewhere (but not if we're already closing from a menu item)
        if (!_isClosing)
        {
            Close();
        }
    }
    
    private void SafeClose()
    {
        _isClosing = true;
        Close();
    }
    
    private void Settings_Click(object sender, MouseButtonEventArgs e)
    {
        SettingsRequested = true;
        SafeClose();
    }
    
    private void ChangePosition_Click(object sender, MouseButtonEventArgs e)
    {
        ChangePositionRequested = true;
        SafeClose();
    }
    
    private void AutoStart_Click(object sender, MouseButtonEventArgs e)
    {
        _isAutoStartEnabled = !_isAutoStartEnabled;
        AutoStartToggled = true;
        UpdateAutoStartCheckbox();
        
        // Apply change immediately
        AutoStartHelper.SetAutoStart(_isAutoStartEnabled);
        
        // Don't close menu, let user see the change
        e.Handled = true;
    }
    
    private void About_Click(object sender, MouseButtonEventArgs e)
    {
        AboutRequested = true;
        SafeClose();
    }
    
    private void Exit_Click(object sender, MouseButtonEventArgs e)
    {
        ExitRequested = true;
        SafeClose();
    }
}
