using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FajrApp.Models;
using FajrApp.Services;

namespace FajrApp;

public class CitySearchResult
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Country { get; set; } = "";
    
    public string DisplayName => 
        string.IsNullOrEmpty(State) 
            ? $"{City}, {Country}" 
            : $"{City}, {State}, {Country}";
}

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
    private bool _isInitializing = true;
    
    private Brush TextColor => _settings.Theme == AppTheme.Light 
        ? new SolidColorBrush(Color.FromRgb(30, 30, 30)) 
        : new SolidColorBrush(Color.FromRgb(224, 224, 224));
    
    // Method keys for localization
    private static readonly Dictionary<CalculationMethod, string> MethodKeys = new()
    {
        { CalculationMethod.Russia, "MethodRussia" },
        { CalculationMethod.MuslimWorldLeague, "MethodMuslimWorldLeague" },
        { CalculationMethod.Egyptian, "MethodEgypt" },
        { CalculationMethod.Karachi, "MethodKarachi" },
        { CalculationMethod.UmmAlQura, "MethodMakkah" },
        { CalculationMethod.Dubai, "MethodDubai" },
        { CalculationMethod.MoonsightingCommittee, "MethodMoonsighting" },
        { CalculationMethod.ISNA, "MethodISNA" },
        { CalculationMethod.Kuwait, "MethodKuwait" },
        { CalculationMethod.Qatar, "MethodQatar" },
        { CalculationMethod.Singapore, "MethodSingapore" },
        { CalculationMethod.Tehran, "MethodTehran" },
        { CalculationMethod.Turkey, "MethodTurkey" }
    };
    
    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        
        LoadSettings();
        PopulateLanguageComboBox();
        PopulateMethodComboBox();
        UpdateLocalization();
        
        // Apply theme after window is loaded
        Loaded += (s, e) => 
        {
            Helpers.ThemeManager.ApplyTheme(this, _settings);
            ApplyTabButtonStyles();
        };
        
        // Show initial tab (Language)
        ShowTab("Language");
        
        _isInitializing = false;
    }
    
    private void ApplyTabButtonStyles()
    {
        var buttons = new[] { LanguageTabButton, LocationTabButton, MethodTabButton, 
                              OffsetsTabButton, NotificationsTabButton, AppearanceTabButton };
        
        // Get the appropriate style based on theme
        var styleName = _settings.Theme == AppTheme.Light ? "TabButtonStyleLight" : "TabButtonStyleDark";
        var style = (Style)FindResource(styleName);
        
        foreach (var button in buttons)
        {
            button.Style = style;
        }
    }
    
    private void RemoveFromParent(FrameworkElement element)
    {
        if (element.Parent is Panel panel)
        {
            panel.Children.Remove(element);
        }
        else if (element.Parent is ContentControl cc)
        {
            cc.Content = null;
        }
        else if (element.Parent is Decorator decorator)
        {
            decorator.Child = null;
        }
    }
    
    private void AddToContent(FrameworkElement element)
    {
        RemoveFromParent(element);
        ContentPanel.Children.Add(element);
    }
    
    private void TabButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // Update button states
            LanguageTabButton.Tag = null;
            LocationTabButton.Tag = null;
            MethodTabButton.Tag = null;
            OffsetsTabButton.Tag = null;
            NotificationsTabButton.Tag = null;
            AppearanceTabButton.Tag = null;
            
            button.Tag = "Selected";
            
            // Apply correct foreground colors based on theme
            ApplyTabButtonStyles();
            
            // Show corresponding content
            var tabName = button.Name.Replace("TabButton", "");
            ShowTab(tabName);
        }
    }
    
    private void ShowTab(string tabName)
    {
        ContentPanel.Children.Clear();
        
        switch (tabName)
        {
            case "Language":
                ShowLanguageTab();
                break;
            case "Location":
                ShowLocationTab();
                break;
            case "Method":
                ShowMethodTab();
                break;
            case "Offsets":
                ShowOffsetsTab();
                break;
            case "Notifications":
                ShowNotificationsTab();
                break;
            case "Appearance":
                ShowAppearanceTab();
                break;
        }
    }
    
    private void ShowLanguageTab()
    {
        var header = new TextBlock
        {
            Text = LocalizationService.T("Language"),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = TextColor,
            Margin = new Thickness(0, 0, 0, 16)
        };
        ContentPanel.Children.Add(header);
        
        var label = new TextBlock
        {
            Text = LocalizationService.T("Language"),
            Foreground = TextColor,
            Margin = new Thickness(0, 0, 0, 4)
        };
        ContentPanel.Children.Add(label);
        AddToContent(LanguageComboBox);
    }
    
    private void ShowLocationTab()
    {
        var header = new TextBlock
        {
            Text = LocalizationService.T("Location"),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = TextColor,
            Margin = new Thickness(0, 0, 0, 16)
        };
        ContentPanel.Children.Add(header);
        
        // City search section
        var cityLabel = new TextBlock { Text = CityLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 0, 0, 4) };
        ContentPanel.Children.Add(cityLabel);
        
        var searchGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
        searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        
        RemoveFromParent(CityTextBox);
        RemoveFromParent(SearchButton);
        Grid.SetColumn(CityTextBox, 0);
        Grid.SetColumn(SearchButton, 2);
        searchGrid.Children.Add(CityTextBox);
        searchGrid.Children.Add(SearchButton);
        ContentPanel.Children.Add(searchGrid);
        
        // Results listbox
        AddToContent(CityResultsListBox);
        
        // Find location button
        AddToContent(FindLocationButton);
        
        // Status text
        AddToContent(StatusText);
        
        // Coordinates
        var coordLabel = new TextBlock
        {
            Text = LocalizationService.T("Coordinates"),
            FontWeight = FontWeights.SemiBold,
            Foreground = TextColor,
            Margin = new Thickness(0, 16, 0, 8)
        };
        ContentPanel.Children.Add(coordLabel);
        
        var coordGrid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        coordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        coordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
        coordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        
        var latStack = new StackPanel();
        latStack.Children.Add(new TextBlock { Text = LatitudeLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 0, 0, 4) });
        RemoveFromParent(LatitudeTextBox);
        latStack.Children.Add(LatitudeTextBox);
        Grid.SetColumn(latStack, 0);
        
        var lonStack = new StackPanel();
        lonStack.Children.Add(new TextBlock { Text = LongitudeLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 0, 0, 4) });
        RemoveFromParent(LongitudeTextBox);
        lonStack.Children.Add(LongitudeTextBox);
        Grid.SetColumn(lonStack, 2);
        
        coordGrid.Children.Add(latStack);
        coordGrid.Children.Add(lonStack);
        ContentPanel.Children.Add(coordGrid);
    }
    
    private void ShowMethodTab()
    {
        var header = new TextBlock
        {
            Text = LocalizationService.T("CalculationMethod"),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = TextColor,
            Margin = new Thickness(0, 0, 0, 16)
        };
        ContentPanel.Children.Add(header);
        
        var methodLabel = new TextBlock { Text = MethodLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 0, 0, 4) };
        ContentPanel.Children.Add(methodLabel);
        AddToContent(MethodComboBox);
        
        var asrLabel = new TextBlock { Text = AsrMethodLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 16, 0, 4) };
        ContentPanel.Children.Add(asrLabel);
        AddToContent(AsrMethodComboBox);
    }
    
    private void ShowOffsetsTab()
    {
        var header = new TextBlock
        {
            Text = LocalizationService.T("TimeOffsets"),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = TextColor,
            Margin = new Thickness(0, 0, 0, 16)
        };
        ContentPanel.Children.Add(header);
        
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        // Row 0
        var fajrStack = new StackPanel();
        fajrStack.Children.Add(new TextBlock { Text = FajrLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 0, 0, 4) });
        RemoveFromParent(FajrOffsetTextBox);
        fajrStack.Children.Add(FajrOffsetTextBox);
        Grid.SetColumn(fajrStack, 0);
        Grid.SetRow(fajrStack, 0);
        
        var sunriseStack = new StackPanel();
        sunriseStack.Children.Add(new TextBlock { Text = SunriseLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 0, 0, 4) });
        RemoveFromParent(SunriseOffsetTextBox);
        sunriseStack.Children.Add(SunriseOffsetTextBox);
        Grid.SetColumn(sunriseStack, 2);
        Grid.SetRow(sunriseStack, 0);
        
        var dhuhrStack = new StackPanel();
        dhuhrStack.Children.Add(new TextBlock { Text = DhuhrLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 0, 0, 4) });
        RemoveFromParent(DhuhrOffsetTextBox);
        dhuhrStack.Children.Add(DhuhrOffsetTextBox);
        Grid.SetColumn(dhuhrStack, 4);
        Grid.SetRow(dhuhrStack, 0);
        
        // Row 2
        var asrStack = new StackPanel();
        asrStack.Children.Add(new TextBlock { Text = AsrLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 0, 0, 4) });
        RemoveFromParent(AsrOffsetTextBox);
        asrStack.Children.Add(AsrOffsetTextBox);
        Grid.SetColumn(asrStack, 0);
        Grid.SetRow(asrStack, 2);
        
        var maghribStack = new StackPanel();
        maghribStack.Children.Add(new TextBlock { Text = MaghribLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 0, 0, 4) });
        RemoveFromParent(MaghribOffsetTextBox);
        maghribStack.Children.Add(MaghribOffsetTextBox);
        Grid.SetColumn(maghribStack, 2);
        Grid.SetRow(maghribStack, 2);
        
        var ishaStack = new StackPanel();
        ishaStack.Children.Add(new TextBlock { Text = IshaLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 0, 0, 4) });
        RemoveFromParent(IshaOffsetTextBox);
        ishaStack.Children.Add(IshaOffsetTextBox);
        Grid.SetColumn(ishaStack, 4);
        Grid.SetRow(ishaStack, 2);
        
        grid.Children.Add(fajrStack);
        grid.Children.Add(sunriseStack);
        grid.Children.Add(dhuhrStack);
        grid.Children.Add(asrStack);
        grid.Children.Add(maghribStack);
        grid.Children.Add(ishaStack);
        
        ContentPanel.Children.Add(grid);
    }
    
    private void ShowNotificationsTab()
    {
        var header = new TextBlock
        {
            Text = LocalizationService.T("Notifications"),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = TextColor,
            Margin = new Thickness(0, 0, 0, 16)
        };
        ContentPanel.Children.Add(header);
        
        AddToContent(NotificationsEnabledCheckBox);
        
        var soundLabel = new TextBlock { Text = NotificationSoundLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 16, 0, 4) };
        ContentPanel.Children.Add(soundLabel);
        AddToContent(NotificationSoundComboBox);
        
        AddToContent(AzanHintText);
        AddToContent(OpenSoundsFolderButton);
    }
    
    private void ShowAppearanceTab()
    {
        var header = new TextBlock
        {
            Text = LocalizationService.T("Appearance"),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = TextColor,
            Margin = new Thickness(0, 0, 0, 16)
        };
        ContentPanel.Children.Add(header);
        
        var themeLabel = new TextBlock { Text = ThemeLabel.Text, Foreground = TextColor, Margin = new Thickness(0, 0, 0, 4) };
        ContentPanel.Children.Add(themeLabel);
        AddToContent(ThemeComboBox);
        
        var opacityGrid = new Grid { Margin = new Thickness(0, 16, 0, 4) };
        opacityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        opacityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        RemoveFromParent(OpacityLabel);
        RemoveFromParent(OpacityValueText);
        opacityGrid.Children.Add(OpacityLabel);
        Grid.SetColumn(OpacityValueText, 1);
        opacityGrid.Children.Add(OpacityValueText);
        ContentPanel.Children.Add(opacityGrid);
        AddToContent(OpacitySlider);
    }
    
    private void UpdateLocalization()
    {
        Title = LocalizationService.T("SettingsTitle");
        
        // Tab buttons
        LanguageTabButton.Content = $"🌐 {LocalizationService.T("Language")}";
        LocationTabButton.Content = $"📍 {LocalizationService.T("Location")}";
        MethodTabButton.Content = $"🕌 {LocalizationService.T("CalculationMethod")}";
        OffsetsTabButton.Content = $"⏱️ {LocalizationService.T("TimeOffsets")}";
        NotificationsTabButton.Content = $"🔔 {LocalizationService.T("Notifications")}";
        AppearanceTabButton.Content = $"🎨 {LocalizationService.T("Appearance")}";
        
        // Language section
        LanguageLabel.Text = LocalizationService.T("Language");
        
        // Location section
        CityLabel.Text = $"{LocalizationService.T("City")} ({LocalizationService.T("SearchCity")})";
        SearchButton.Content = $"🔍 {LocalizationService.T("Search")}";
        FindLocationButton.Content = $"📍 {LocalizationService.T("Location")}";
        LatitudeLabel.Text = LocalizationService.T("Latitude");
        LongitudeLabel.Text = LocalizationService.T("Longitude");
        
        // Method section
        MethodLabel.Text = LocalizationService.T("CalculationMethod");
        AsrMethodLabel.Text = LocalizationService.T("AsrCalculation");
        AsrStandardItem.Content = LocalizationService.T("Standard");
        AsrHanafiItem.Content = LocalizationService.T("Hanafi");
        
        // Offsets section
        FajrLabel.Text = LocalizationService.T("Fajr");
        SunriseLabel.Text = LocalizationService.T("Sunrise");
        DhuhrLabel.Text = LocalizationService.T("Dhuhr");
        AsrLabel.Text = LocalizationService.T("Asr");
        MaghribLabel.Text = LocalizationService.T("Maghrib");
        IshaLabel.Text = LocalizationService.T("Isha");
        
        // Notifications section
        NotificationsEnabledCheckBox.Content = LocalizationService.T("EnableNotifications");
        NotificationSoundLabel.Text = LocalizationService.T("NotificationSound");
        SoundNoneItem.Content = LocalizationService.T("SoundNone");
        SoundSystemItem.Content = LocalizationService.T("SoundSystem");
        SoundAzanItem.Content = LocalizationService.T("SoundAzan");
        AzanHintText.Text = $"💡 {LocalizationService.T("AzanHint")}";
        OpenSoundsFolderButton.Content = $"📁 {LocalizationService.T("OpenSoundsFolder")}";
        
        // Appearance section
        ThemeLabel.Text = LocalizationService.T("Theme");
        ThemeLightItem.Content = LocalizationService.T("ThemeLight");
        ThemeDarkItem.Content = LocalizationService.T("ThemeDark");
        OpacityLabel.Text = LocalizationService.T("WidgetOpacity");
        
        // Buttons
        CancelButton.Content = LocalizationService.T("Cancel");
        SaveButton.Content = LocalizationService.T("Save");
        
        // Refresh method combobox with localized names
        RefreshMethodComboBox();
    }
    
    private void PopulateLanguageComboBox()
    {
        foreach (var lang in LocalizationService.AvailableLanguages)
        {
            var item = new ComboBoxItem
            {
                Content = lang.Value,
                Tag = lang.Key
            };
            
            LanguageComboBox.Items.Add(item);
            
            if (lang.Key == LocalizationService.CurrentLanguage)
            {
                LanguageComboBox.SelectedItem = item;
            }
        }
    }
    
    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        
        if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            var langCode = selectedItem.Tag?.ToString() ?? "en";
            LocalizationService.SetLanguage(langCode);
            UpdateLocalization();
        }
    }
    
    private void LoadSettings()
    {
        LatitudeTextBox.Text = _settings.Latitude.ToString(CultureInfo.InvariantCulture);
        LongitudeTextBox.Text = _settings.Longitude.ToString(CultureInfo.InvariantCulture);
        CityTextBox.Text = _settings.City;
        
        FajrOffsetTextBox.Text = _settings.FajrOffset.ToString();
        SunriseOffsetTextBox.Text = _settings.SunriseOffset.ToString();
        DhuhrOffsetTextBox.Text = _settings.DhuhrOffset.ToString();
        AsrOffsetTextBox.Text = _settings.AsrOffset.ToString();
        MaghribOffsetTextBox.Text = _settings.MaghribOffset.ToString();
        IshaOffsetTextBox.Text = _settings.IshaOffset.ToString();
        
        // Asr method
        AsrMethodComboBox.SelectedIndex = (int)_settings.AsrMethod;
        
        // Notification settings
        NotificationsEnabledCheckBox.IsChecked = _settings.NotificationsEnabled;
        NotificationSoundComboBox.SelectedIndex = (int)_settings.NotificationSound;
        
        // Appearance settings
        ThemeComboBox.SelectedIndex = (int)_settings.Theme;
        OpacitySlider.Value = _settings.WidgetOpacity * 100;
    }
    
    private void PopulateMethodComboBox()
    {
        foreach (var method in MethodKeys)
        {
            var item = new ComboBoxItem
            {
                Content = LocalizationService.T(method.Value),
                Tag = method.Key
            };
            
            MethodComboBox.Items.Add(item);
            
            if (method.Key == _settings.Method)
            {
                MethodComboBox.SelectedItem = item;
            }
        }
    }
    
    private void RefreshMethodComboBox()
    {
        var selectedMethod = _settings.Method;
        if (MethodComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            selectedMethod = (CalculationMethod)selectedItem.Tag;
        }
        
        MethodComboBox.Items.Clear();
        
        foreach (var method in MethodKeys)
        {
            var item = new ComboBoxItem
            {
                Content = LocalizationService.T(method.Value),
                Tag = method.Key
            };
            
            MethodComboBox.Items.Add(item);
            
            if (method.Key == selectedMethod)
            {
                MethodComboBox.SelectedItem = item;
            }
        }
    }
    
    private void CityTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchCity_Click(sender, e);
        }
    }
    
    private async void SearchCity_Click(object sender, RoutedEventArgs e)
    {
        var cityName = CityTextBox.Text.Trim();
        if (string.IsNullOrEmpty(cityName))
        {
            StatusText.Text = LocalizationService.T("EnterCityName");
            StatusText.Foreground = System.Windows.Media.Brushes.Orange;
            return;
        }
        
        StatusText.Text = LocalizationService.T("SearchingCity");
        StatusText.Foreground = System.Windows.Media.Brushes.Gray;
        
        try
        {
            var results = await SearchCityAsync(cityName);
            if (results.Count > 0)
            {
                // Show results in ListBox
                CityResultsListBox.ItemsSource = results;
                CityResultsListBox.Visibility = Visibility.Visible;
                
                StatusText.Text = string.Format(LocalizationService.T("CitiesFound"), results.Count);
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                CityResultsListBox.Visibility = Visibility.Collapsed;
                StatusText.Text = LocalizationService.T("CityNotFound");
                StatusText.Foreground = System.Windows.Media.Brushes.Orange;
            }
        }
        catch (Exception ex)
        {
            CityResultsListBox.Visibility = Visibility.Collapsed;
            StatusText.Text = string.Format(LocalizationService.T("SearchError"), ex.Message);
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
        }
    }
    
    private void CityResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CityResultsListBox.SelectedItem is CitySearchResult result)
        {
            LatitudeTextBox.Text = result.Latitude.ToString(CultureInfo.InvariantCulture);
            LongitudeTextBox.Text = result.Longitude.ToString(CultureInfo.InvariantCulture);
            CityTextBox.Text = result.City;
            
            // Hide the results list
            CityResultsListBox.Visibility = Visibility.Collapsed;
            
            StatusText.Text = string.Format(LocalizationService.T("CitySelected"), result.DisplayName);
            StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
        }
    }
    
    private async Task<List<CitySearchResult>> SearchCityAsync(string cityName)
    {
        var results = new List<CitySearchResult>();
        
        // Using Nominatim OpenStreetMap API for geocoding (free, no API key needed)
        var encodedCity = Uri.EscapeDataString(cityName);
        var url = $"https://nominatim.openstreetmap.org/search?q={encodedCity}&format=json&limit=10&addressdetails=1";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "FajrApp/1.0 (Prayer Times Widget)");
        
        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var jsonResults = JsonDocument.Parse(json);
        
        foreach (var item in jsonResults.RootElement.EnumerateArray())
        {
            try
            {
                var lat = double.Parse(item.GetProperty("lat").GetString()!, CultureInfo.InvariantCulture);
                var lon = double.Parse(item.GetProperty("lon").GetString()!, CultureInfo.InvariantCulture);
                
                string city = "";
                string state = "";
                string country = "";
                
                if (item.TryGetProperty("address", out var address))
                {
                    // Try to get city name
                    if (address.TryGetProperty("city", out var cityProp))
                        city = cityProp.GetString() ?? "";
                    else if (address.TryGetProperty("town", out var townProp))
                        city = townProp.GetString() ?? "";
                    else if (address.TryGetProperty("village", out var villageProp))
                        city = villageProp.GetString() ?? "";
                    else if (address.TryGetProperty("municipality", out var municipalityProp))
                        city = municipalityProp.GetString() ?? "";
                    
                    // Get state/region
                    if (address.TryGetProperty("state", out var stateProp))
                        state = stateProp.GetString() ?? "";
                    else if (address.TryGetProperty("region", out var regionProp))
                        state = regionProp.GetString() ?? "";
                    else if (address.TryGetProperty("county", out var countyProp))
                        state = countyProp.GetString() ?? "";
                    
                    // Get country
                    if (address.TryGetProperty("country", out var countryProp))
                        country = countryProp.GetString() ?? "";
                }
                
                // If we don't have a city name, use the first part of display_name
                if (string.IsNullOrEmpty(city) && item.TryGetProperty("display_name", out var displayName))
                {
                    var parts = displayName.GetString()?.Split(',');
                    if (parts?.Length > 0)
                        city = parts[0].Trim();
                }
                
                if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(country))
                {
                    results.Add(new CitySearchResult
                    {
                        Latitude = lat,
                        Longitude = lon,
                        City = city,
                        State = state,
                        Country = country
                    });
                }
            }
            catch
            {
                // Skip invalid results
                continue;
            }
        }
        
        return results;
    }
    
    private async void FindLocation_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "📍 Определение местоположения...";
        StatusText.Foreground = System.Windows.Media.Brushes.Gray;
        FindLocationButton.IsEnabled = false;
        
        try
        {
            var location = await GetDeviceLocationAsync();
            
            if (location.HasValue)
            {
                LatitudeTextBox.Text = location.Value.Lat.ToString(CultureInfo.InvariantCulture);
                LongitudeTextBox.Text = location.Value.Lon.ToString(CultureInfo.InvariantCulture);
                
                // Try to reverse geocode to get city name
                var cityName = await ReversGeocodeAsync(location.Value.Lat, location.Value.Lon);
                if (!string.IsNullOrEmpty(cityName))
                {
                    CityTextBox.Text = cityName;
                }
                
                StatusText.Text = $"✅ Местоположение определено!";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                // Fallback: try IP-based geolocation
                var ipLocation = await GetLocationByIpAsync();
                if (ipLocation.HasValue)
                {
                    LatitudeTextBox.Text = ipLocation.Value.Lat.ToString(CultureInfo.InvariantCulture);
                    LongitudeTextBox.Text = ipLocation.Value.Lon.ToString(CultureInfo.InvariantCulture);
                    CityTextBox.Text = ipLocation.Value.City;
                    
                    StatusText.Text = $"✅ Определено по IP: {ipLocation.Value.City}";
                    StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
                }
                else
                {
                    StatusText.Text = "❌ Не удалось определить местоположение. Укажите город вручную.";
                    StatusText.Foreground = System.Windows.Media.Brushes.Orange;
                }
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"❌ Ошибка: {ex.Message}";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
        }
        finally
        {
            FindLocationButton.IsEnabled = true;
        }
    }
    
    private async Task<(double Lat, double Lon)?> GetDeviceLocationAsync()
    {
        // Windows.Devices.Geolocation requires Windows SDK
        // Using IP-based geolocation as fallback
        var ipLocation = await GetLocationByIpAsync();
        if (ipLocation.HasValue)
        {
            return (ipLocation.Value.Lat, ipLocation.Value.Lon);
        }
        return null;
    }
    
    private async Task<(double Lat, double Lon, string City)?> GetLocationByIpAsync()
    {
        try
        {
            // Using ip-api.com (free, no API key needed)
            var url = "http://ip-api.com/json/?fields=status,city,lat,lon";
            var response = await HttpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            if (json.RootElement.GetProperty("status").GetString() == "success")
            {
                var lat = json.RootElement.GetProperty("lat").GetDouble();
                var lon = json.RootElement.GetProperty("lon").GetDouble();
                var city = json.RootElement.GetProperty("city").GetString() ?? "Unknown";
                return (lat, lon, city);
            }
        }
        catch
        {
            // IP geolocation failed
        }
        
        return null;
    }
    
    private async Task<string?> ReversGeocodeAsync(double lat, double lon)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/reverse?lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lon.ToString(CultureInfo.InvariantCulture)}&format=json";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "FajrApp/1.0 (Prayer Times Widget)");
            
            var response = await HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(json);
            
            if (result.RootElement.TryGetProperty("address", out var address))
            {
                // Try to get city name
                if (address.TryGetProperty("city", out var city))
                    return city.GetString();
                if (address.TryGetProperty("town", out var town))
                    return town.GetString();
                if (address.TryGetProperty("village", out var village))
                    return village.GetString();
                if (address.TryGetProperty("state", out var state))
                    return state.GetString();
            }
        }
        catch
        {
            // Reverse geocoding failed
        }
        
        return null;
    }
    
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate and save settings
            if (double.TryParse(LatitudeTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat))
            {
                _settings.Latitude = lat;
            }
            
            if (double.TryParse(LongitudeTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var lng))
            {
                _settings.Longitude = lng;
            }
            
            _settings.City = CityTextBox.Text;
            
            // Method
            if (MethodComboBox.SelectedItem is ComboBoxItem selectedMethod)
            {
                _settings.Method = (CalculationMethod)selectedMethod.Tag;
            }
            
            // Asr method
            if (AsrMethodComboBox.SelectedItem is ComboBoxItem selectedAsrMethod)
            {
                _settings.AsrMethod = (AsrMethod)int.Parse(selectedAsrMethod.Tag?.ToString() ?? "0");
            }
            
            // Offsets
            if (int.TryParse(FajrOffsetTextBox.Text, out var fajrOffset))
                _settings.FajrOffset = fajrOffset;
            if (int.TryParse(SunriseOffsetTextBox.Text, out var sunriseOffset))
                _settings.SunriseOffset = sunriseOffset;
            if (int.TryParse(DhuhrOffsetTextBox.Text, out var dhuhrOffset))
                _settings.DhuhrOffset = dhuhrOffset;
            if (int.TryParse(AsrOffsetTextBox.Text, out var asrOffset))
                _settings.AsrOffset = asrOffset;
            if (int.TryParse(MaghribOffsetTextBox.Text, out var maghribOffset))
                _settings.MaghribOffset = maghribOffset;
            if (int.TryParse(IshaOffsetTextBox.Text, out var ishaOffset))
                _settings.IshaOffset = ishaOffset;
            
            // Notification settings
            _settings.NotificationsEnabled = NotificationsEnabledCheckBox.IsChecked ?? true;
            if (NotificationSoundComboBox.SelectedItem is ComboBoxItem selectedSound)
            {
                _settings.NotificationSound = (NotificationSoundType)int.Parse(selectedSound.Tag?.ToString() ?? "1");
            }
            
            // Appearance settings
            if (ThemeComboBox.SelectedItem is ComboBoxItem selectedTheme)
            {
                _settings.Theme = (AppTheme)int.Parse(selectedTheme.Tag?.ToString() ?? "1");
            }
            _settings.WidgetOpacity = OpacitySlider.Value / 100.0;
            
            // Clear cache when settings change
            _settings.CachedTimes = null;
            
            SettingsService.Save(_settings);
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
    
    private void OpenSoundsFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            NotificationService.EnsureSoundsFolderExists();
            var soundsFolder = NotificationService.GetSoundsFolder();
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = soundsFolder,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening folder: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OpacityValueText != null)
        {
            OpacityValueText.Text = $"{(int)e.NewValue}%";
        }
    }
}
