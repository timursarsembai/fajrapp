using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using FajrApp.Models;

namespace FajrApp.Services;

public class AppSettings
{
    public double Latitude { get; set; } = 55.7558;  // Moscow default
    public double Longitude { get; set; } = 37.6173;
    public string City { get; set; } = "Москва";
    public CalculationMethod Method { get; set; } = CalculationMethod.Russia;
    public AsrMethod AsrMethod { get; set; } = AsrMethod.Standard;
    public bool UseGeolocation { get; set; } = true;
    public bool AutoStart { get; set; } = true;
    
    // Widget position offset from default (in pixels)
    public double WidgetOffsetX { get; set; } = 0;
    
    // Time offsets in minutes
    public int FajrOffset { get; set; } = 0;
    public int SunriseOffset { get; set; } = 0;
    public int DhuhrOffset { get; set; } = 0;
    public int AsrOffset { get; set; } = 0;
    public int MaghribOffset { get; set; } = 0;
    public int IshaOffset { get; set; } = 0;
    
    // Cache
    public CachedPrayerTimes? CachedTimes { get; set; }
}

public class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FajrApp",
        "settings.json"
    );
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
    
    private static AppSettings? _instance;
    
    public static AppSettings Load()
    {
        if (_instance != null)
            return _instance;
            
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                _instance = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            else
            {
                _instance = new AppSettings();
                Save(_instance);
            }
        }
        catch
        {
            _instance = new AppSettings();
        }
        
        return _instance;
    }
    
    public static void Save(AppSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsPath, json);
            _instance = settings;
        }
        catch
        {
            // Silently fail
        }
    }
    
    public static void UpdateCache(PrayerTimes times)
    {
        var settings = Load();
        settings.CachedTimes = new CachedPrayerTimes
        {
            CacheDate = DateTime.Today,
            Times = times
        };
        Save(settings);
    }
    
    public static PrayerTimes? GetCachedTimes()
    {
        var settings = Load();
        if (settings.CachedTimes != null && 
            settings.CachedTimes.CacheDate.Date == DateTime.Today &&
            settings.CachedTimes.Times != null)
        {
            return settings.CachedTimes.Times;
        }
        return null;
    }
}
