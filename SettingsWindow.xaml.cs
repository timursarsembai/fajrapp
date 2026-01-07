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
    
    private static readonly Dictionary<CalculationMethod, string> MethodNames = new()
    {
        { CalculationMethod.Russia, "Духовное управление мусульман России" },
        { CalculationMethod.MuslimWorldLeague, "Всемирная исламская лига (MWL)" },
        { CalculationMethod.Egyptian, "Египетский общий орган геодезии" },
        { CalculationMethod.Karachi, "Карачи (Университет исламских наук)" },
        { CalculationMethod.UmmAlQura, "Умм аль-Кура, Мекка" },
        { CalculationMethod.Dubai, "Дубай" },
        { CalculationMethod.MoonsightingCommittee, "Комитет Moonsighting" },
        { CalculationMethod.ISNA, "ISNA (Северная Америка)" },
        { CalculationMethod.Kuwait, "Кувейт" },
        { CalculationMethod.Qatar, "Катар" },
        { CalculationMethod.Singapore, "Сингапур" },
        { CalculationMethod.Tehran, "Тегеран" },
        { CalculationMethod.Turkey, "Турция" }
    };
    
    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        
        LoadSettings();
        PopulateMethodComboBox();
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
    }
    
    private void PopulateMethodComboBox()
    {
        foreach (var method in MethodNames)
        {
            var item = new ComboBoxItem
            {
                Content = method.Value,
                Tag = method.Key
            };
            
            MethodComboBox.Items.Add(item);
            
            if (method.Key == _settings.Method)
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
}
