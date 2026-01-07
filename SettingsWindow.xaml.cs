using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FajrApp.Models;
using FajrApp.Services;

namespace FajrApp;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
    private bool _isInitializing = true;
    
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
        
        _isInitializing = false;
    }
    
    private void UpdateLocalization()
    {
        Title = LocalizationService.T("SettingsTitle");
        HeaderText.Text = $"⚙️ {LocalizationService.T("Settings")}";
        
        // Language section
        LanguageSectionHeader.Text = $"🌐 {LocalizationService.T("Language")}";
        LanguageLabel.Text = LocalizationService.T("Language");
        
        // Location section
        LocationSectionHeader.Text = $"📍 {LocalizationService.T("Location")}";
        CityLabel.Text = $"{LocalizationService.T("City")} ({LocalizationService.T("SearchCity")})";
        SearchButton.Content = $"🔍 {LocalizationService.T("Search")}";
        FindLocationButton.Content = $"📍 {LocalizationService.T("Location")}";
        LatitudeLabel.Text = LocalizationService.T("Latitude");
        LongitudeLabel.Text = LocalizationService.T("Longitude");
        
        // Method section
        MethodSectionHeader.Text = $"🕌 {LocalizationService.T("CalculationMethod")}";
        MethodLabel.Text = LocalizationService.T("CalculationMethod");
        AsrMethodLabel.Text = LocalizationService.T("AsrCalculation");
        AsrStandardItem.Content = LocalizationService.T("Standard");
        AsrHanafiItem.Content = LocalizationService.T("Hanafi");
        
        // Offsets section
        OffsetsSectionHeader.Text = $"⏱️ {LocalizationService.T("TimeOffsets")}";
        FajrLabel.Text = LocalizationService.T("Fajr");
        SunriseLabel.Text = LocalizationService.T("Sunrise");
        DhuhrLabel.Text = LocalizationService.T("Dhuhr");
        AsrLabel.Text = LocalizationService.T("Asr");
        MaghribLabel.Text = LocalizationService.T("Maghrib");
        IshaLabel.Text = LocalizationService.T("Isha");
        
        // Notifications section
        NotificationsSectionHeader.Text = $"🔔 {LocalizationService.T("Notifications")}";
        NotificationsEnabledCheckBox.Content = LocalizationService.T("EnableNotifications");
        NotificationSoundLabel.Text = LocalizationService.T("NotificationSound");
        SoundNoneItem.Content = LocalizationService.T("SoundNone");
        SoundSystemItem.Content = LocalizationService.T("SoundSystem");
        SoundAzanItem.Content = LocalizationService.T("SoundAzan");
        AzanHintText.Text = $"💡 {LocalizationService.T("AzanHint")}";
        OpenSoundsFolderButton.Content = $"📁 {LocalizationService.T("OpenSoundsFolder")}";
        
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
            StatusText.Text = "Введите название города";
            StatusText.Foreground = System.Windows.Media.Brushes.Orange;
            return;
        }
        
        StatusText.Text = "🔍 Поиск города...";
        StatusText.Foreground = System.Windows.Media.Brushes.Gray;
        
        try
        {
            var result = await SearchCityAsync(cityName);
            if (result.HasValue)
            {
                LatitudeTextBox.Text = result.Value.Lat.ToString(CultureInfo.InvariantCulture);
                LongitudeTextBox.Text = result.Value.Lon.ToString(CultureInfo.InvariantCulture);
                CityTextBox.Text = result.Value.DisplayName;
                
                StatusText.Text = $"✅ Найден: {result.Value.DisplayName}";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                StatusText.Text = "❌ Город не найден. Попробуйте другое название.";
                StatusText.Foreground = System.Windows.Media.Brushes.Orange;
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"❌ Ошибка поиска: {ex.Message}";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
        }
    }
    
    private async Task<(double Lat, double Lon, string DisplayName)?> SearchCityAsync(string cityName)
    {
        // Using Nominatim OpenStreetMap API for geocoding (free, no API key needed)
        var encodedCity = Uri.EscapeDataString(cityName);
        var url = $"https://nominatim.openstreetmap.org/search?q={encodedCity}&format=json&limit=1&addressdetails=1";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "FajrApp/1.0 (Prayer Times Widget)");
        
        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var results = JsonDocument.Parse(json);
        
        if (results.RootElement.GetArrayLength() > 0)
        {
            var first = results.RootElement[0];
            var lat = double.Parse(first.GetProperty("lat").GetString()!, CultureInfo.InvariantCulture);
            var lon = double.Parse(first.GetProperty("lon").GetString()!, CultureInfo.InvariantCulture);
            var displayName = first.GetProperty("display_name").GetString() ?? cityName;
            
            // Shorten display name (take first 2-3 parts)
            var parts = displayName.Split(',');
            displayName = parts.Length >= 2 
                ? $"{parts[0].Trim()}, {parts[^1].Trim()}" 
                : parts[0].Trim();
            
            return (lat, lon, displayName);
        }
        
        return null;
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
}
