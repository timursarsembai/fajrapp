using System;
using System.Text.Json.Serialization;

namespace FajrApp.Models;

public class PrayerTimes
{
    public DateTime Fajr { get; set; }
    public DateTime Sunrise { get; set; }
    public DateTime Dhuhr { get; set; }
    public DateTime Asr { get; set; }
    public DateTime Maghrib { get; set; }
    public DateTime Isha { get; set; }
    
    [JsonIgnore]
    public DateTime Date => Fajr.Date;
}

public class CachedPrayerTimes
{
    public DateTime CacheDate { get; set; }
    public PrayerTimes? Times { get; set; }
}

public class PrayerTimeInfo
{
    public string Name { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsNext { get; set; }
}

public enum AsrMethod
{
    Standard = 0,  // Shafi'i, Maliki, Hanbali
    Hanafi = 1     // Hanafi
}

public enum CalculationMethod
{
    MuslimWorldLeague = 3,
    Egyptian = 5,
    Karachi = 1,
    UmmAlQura = 4,
    Dubai = 8,
    MoonsightingCommittee = 12,
    ISNA = 2,
    Kuwait = 9,
    Qatar = 10,
    Singapore = 11,
    Tehran = 7,
    Turkey = 13,
    Russia = 14
}
