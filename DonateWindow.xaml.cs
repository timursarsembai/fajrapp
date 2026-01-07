using System;
using System.Windows;
using System.Windows.Input;
using FajrApp.Services;

namespace FajrApp;

public partial class DonateWindow : Window
{
    private const string DonationAlertsUrl = "https://www.donationalerts.com/r/timursarsembai";
    private const string LiberapayUrl = "https://liberapay.com/timursarsembai/donate";
    
    public DonateWindow()
    {
        InitializeComponent();
        
        UpdateLocalization();
        
        // Close on Escape
        KeyDown += (s, e) =>
        {
            if (e.Key == Key.Escape)
                Close();
        };
        
        // Allow dragging
        MouseLeftButtonDown += (s, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };
        
        // Close when deactivated
        Deactivated += (s, e) => Close();
    }
    
    private void UpdateLocalization()
    {
        Title = LocalizationService.T("DonateTitle");
        HeaderText.Text = $"❤️ {LocalizationService.T("SupportProject")}";
        DescriptionText.Text = LocalizationService.T("DonateDescription");
        ThankYouText.Text = LocalizationService.T("ThankYou");
        CloseButton.Content = LocalizationService.T("Close");
    }
    
    private void DonationAlerts_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl(DonationAlertsUrl);
    }
    
    private void Liberapay_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl(LiberapayUrl);
    }
    
    private void OpenUrl(string url)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail if browser cannot be opened
        }
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
