using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FajrApp.Models;

namespace FajrApp.Services;

public class PrayerService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };
    
    private static readonly Dictionary<string, string> PrayerNames = new()
    {
        { "Fajr", "Фаджр" },
        { "Sunrise", "Восход" },
        { "Dhuhr", "Зухр" },
        { "Asr", "Аср" },
        { "Maghrib", "Магриб" },
        { "Isha", "Иша" }
    };
    
    public static string GetPrayerName(string key) => 
        PrayerNames.TryGetValue(key, out var name) ? name : key;

    public async Task<PrayerTimes?> GetPrayerTimesAsync(AppSettings settings)
    {
        // Check cache first
        var cached = SettingsService.GetCachedTimes();
        if (cached != null)
        {
            return ApplyOffsets(cached, settings);
        }
        
        // Try to get location via Geolocation API
        double lat = settings.Latitude;
        double lng = settings.Longitude;
        
        if (settings.UseGeolocation)
        {
            var location = await TryGetGeolocationAsync();
            if (location.HasValue)
            {
                lat = location.Value.Latitude;
                lng = location.Value.Longitude;
                
                // Update settings with new location
                settings.Latitude = lat;
                settings.Longitude = lng;
                SettingsService.Save(settings);
            }
        }
        
        // Fetch from API
        var times = await FetchFromApiAsync(lat, lng, settings.Method, settings.AsrMethod);
        
        if (times != null)
        {
            SettingsService.UpdateCache(times);
            return ApplyOffsets(times, settings);
        }
        
        return null;
    }
    
    private async Task<(double Latitude, double Longitude)?> TryGetGeolocationAsync()
    {
        try
        {
            // Try to use IP-based geolocation instead of Windows.Devices.Geolocation
            // which requires special SDK packages
            return null; // Fall back to manual coordinates
        }
        catch
        {
            // Geolocation not available, fall back to manual
        }
        
        return null;
    }
    
    private async Task<PrayerTimes?> FetchFromApiAsync(double lat, double lng, CalculationMethod method, AsrMethod asrMethod)
    {
        try
        {
            var today = DateTime.Today;
            var url = $"https://api.aladhan.com/v1/timings/{today:dd-MM-yyyy}" +
                      $"?latitude={lat.ToString(CultureInfo.InvariantCulture)}" +
                      $"&longitude={lng.ToString(CultureInfo.InvariantCulture)}" +
                      $"&method={(int)method}" +
                      $"&school={(int)asrMethod}";
            
            var response = await HttpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            var timings = json.RootElement.GetProperty("data").GetProperty("timings");
            
            return new PrayerTimes
            {
                Fajr = ParseTime(timings.GetProperty("Fajr").GetString()!, today),
                Sunrise = ParseTime(timings.GetProperty("Sunrise").GetString()!, today),
                Dhuhr = ParseTime(timings.GetProperty("Dhuhr").GetString()!, today),
                Asr = ParseTime(timings.GetProperty("Asr").GetString()!, today),
                Maghrib = ParseTime(timings.GetProperty("Maghrib").GetString()!, today),
                Isha = ParseTime(timings.GetProperty("Isha").GetString()!, today)
            };
        }
        catch
        {
            return null;
        }
    }
    
    private DateTime ParseTime(string timeStr, DateTime date)
    {
        // API returns time like "05:30" or "05:30 (EET)"
        var time = timeStr.Split(' ')[0];
        var parts = time.Split(':');
        return date.Date.AddHours(int.Parse(parts[0])).AddMinutes(int.Parse(parts[1]));
    }
    
    private PrayerTimes ApplyOffsets(PrayerTimes times, AppSettings settings)
    {
        return new PrayerTimes
        {
            Fajr = times.Fajr.AddMinutes(settings.FajrOffset),
            Sunrise = times.Sunrise.AddMinutes(settings.SunriseOffset),
            Dhuhr = times.Dhuhr.AddMinutes(settings.DhuhrOffset),
            Asr = times.Asr.AddMinutes(settings.AsrOffset),
            Maghrib = times.Maghrib.AddMinutes(settings.MaghribOffset),
            Isha = times.Isha.AddMinutes(settings.IshaOffset)
        };
    }
    
    public static List<PrayerTimeInfo> GetPrayerList(PrayerTimes times)
    {
        var now = DateTime.Now;
        var list = new List<PrayerTimeInfo>
        {
            new() { Name = GetPrayerName("Fajr"), Time = times.Fajr },
            new() { Name = GetPrayerName("Sunrise"), Time = times.Sunrise },
            new() { Name = GetPrayerName("Dhuhr"), Time = times.Dhuhr },
            new() { Name = GetPrayerName("Asr"), Time = times.Asr },
            new() { Name = GetPrayerName("Maghrib"), Time = times.Maghrib },
            new() { Name = GetPrayerName("Isha"), Time = times.Isha }
        };
        
        // Find current and next prayer
        PrayerTimeInfo? current = null;
        PrayerTimeInfo? next = null;
        
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Time > now)
            {
                next = list[i];
                if (i > 0)
                {
                    current = list[i - 1];
                }
                break;
            }
        }
        
        // If no next prayer today, next is Fajr tomorrow
        if (next == null)
        {
            current = list[^1]; // Isha
            next = list[0]; // Fajr (will be tomorrow)
        }
        
        if (current != null) current.IsCurrent = true;
        if (next != null) next.IsNext = true;
        
        return list;
    }
    
    public static (string Name, DateTime Time, TimeSpan Remaining) GetNextPrayer(PrayerTimes times)
    {
        var now = DateTime.Now;
        var list = new[]
        {
            (GetPrayerName("Fajr"), times.Fajr),
            (GetPrayerName("Sunrise"), times.Sunrise),
            (GetPrayerName("Dhuhr"), times.Dhuhr),
            (GetPrayerName("Asr"), times.Asr),
            (GetPrayerName("Maghrib"), times.Maghrib),
            (GetPrayerName("Isha"), times.Isha)
        };
        
        foreach (var (name, time) in list)
        {
            if (time > now)
            {
                return (name, time, time - now);
            }
        }
        
        // Next is Fajr tomorrow
        var tomorrowFajr = times.Fajr.AddDays(1);
        return (GetPrayerName("Fajr"), tomorrowFajr, tomorrowFajr - now);
    }
    
    public static string GetCurrentPrayerName(PrayerTimes times)
    {
        var now = DateTime.Now;
        
        if (now >= times.Isha) return GetPrayerName("Isha");
        if (now >= times.Maghrib) return GetPrayerName("Maghrib");
        if (now >= times.Asr) return GetPrayerName("Asr");
        if (now >= times.Dhuhr) return GetPrayerName("Dhuhr");
        if (now >= times.Sunrise) return GetPrayerName("Sunrise");
        if (now >= times.Fajr) return GetPrayerName("Fajr");
        
        return GetPrayerName("Isha"); // Before Fajr
    }
}
