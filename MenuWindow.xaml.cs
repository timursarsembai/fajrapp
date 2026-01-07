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
    
    public MenuWindow(bool isAutoStartEnabled)
    {
        InitializeComponent();
        
        _isAutoStartEnabled = isAutoStartEnabled;
        
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
        
        // Start fade in animation
        var animation = (Storyboard)FindResource("FadeInAnimation");
        animation.Begin();
    }
    
    public void PositionNear(Rect widgetRect)
    {
        // Measure content
        Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var menuSize = DesiredSize;
        
        // Position above widget, centered
        var left = widgetRect.Left + (widgetRect.Width / 2) - (menuSize.Width / 2);
        var top = widgetRect.Top - menuSize.Height;
        
        // Ensure visible on screen
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        
        if (left < 8) left = 8;
        if (left + menuSize.Width > screenWidth - 8) left = screenWidth - menuSize.Width - 8;
        if (top < 8) top = widgetRect.Bottom + 8; // Show below if no space above
        
        Left = left;
        Top = top;
    }
    
    private void Window_Deactivated(object sender, EventArgs e)
    {
        // Close when clicking elsewhere
        Close();
    }
    
    private void Settings_Click(object sender, MouseButtonEventArgs e)
    {
        SettingsRequested = true;
        Close();
    }
    
    private void ChangePosition_Click(object sender, MouseButtonEventArgs e)
    {
        ChangePositionRequested = true;
        Close();
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
        Close();
    }
    
    private void Exit_Click(object sender, MouseButtonEventArgs e)
    {
        ExitRequested = true;
        Close();
    }
}
