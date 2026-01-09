using System;
using System.Globalization;

namespace FajrApp.Services;

public static class HijriDateService
{
    private static readonly HijriCalendar _hijriCalendar = new();
    
    // Islamic month names in Arabic
    private static readonly string[] _arabicMonthNames = 
    {
        "محرم", "صفر", "ربيع الأول", "ربيع الثاني",
        "جمادى الأولى", "جمادى الآخرة", "رجب", "شعبان",
        "رمضان", "شوال", "ذو القعدة", "ذو الحجة"
    };
    
    // Islamic month names in English
    private static readonly string[] _englishMonthNames = 
    {
        "Muharram", "Safar", "Rabi' al-Awwal", "Rabi' al-Thani",
        "Jumada al-Ula", "Jumada al-Thani", "Rajab", "Sha'ban",
        "Ramadan", "Shawwal", "Dhu al-Qi'dah", "Dhu al-Hijjah"
    };
    
    // Islamic month names in Russian
    private static readonly string[] _russianMonthNames = 
    {
        "Мухаррам", "Сафар", "Раби уль-авваль", "Раби уль-ахир",
        "Джумада аль-уля", "Джумада аль-ахира", "Раджаб", "Шаабан",
        "Рамадан", "Шавваль", "Зуль-каада", "Зуль-хиджа"
    };
    
    // Islamic month names in Spanish
    private static readonly string[] _spanishMonthNames = 
    {
        "Muharram", "Safar", "Rabi' al-Awwal", "Rabi' al-Thani",
        "Yumada al-Ula", "Yumada al-Thani", "Rayab", "Sha'ban",
        "Ramadán", "Shawwal", "Dhul-Qa'dah", "Dhul-Hiyyah"
    };
    
    // Islamic month names in Indonesian
    private static readonly string[] _indonesianMonthNames = 
    {
        "Muharam", "Safar", "Rabiul Awal", "Rabiul Akhir",
        "Jumadil Awal", "Jumadil Akhir", "Rajab", "Sya'ban",
        "Ramadan", "Syawal", "Dzulkaidah", "Dzulhijjah"
    };
    
    // Islamic month names in Kazakh
    private static readonly string[] _kazakhMonthNames = 
    {
        "Мұхаррам", "Сафар", "Рабиғ ұл-әууәл", "Рабиғ ұл-ахир",
        "Жұмад ұл-ула", "Жұмад ұл-ахира", "Ражаб", "Шағбан",
        "Рамазан", "Шәууәл", "Зұлқағда", "Зұл-хижжа"
    };
    
    public static (int day, int month, int year) GetHijriDate(DateTime gregorianDate)
    {
        int day = _hijriCalendar.GetDayOfMonth(gregorianDate);
        int month = _hijriCalendar.GetMonth(gregorianDate);
        int year = _hijriCalendar.GetYear(gregorianDate);
        
        return (day, month, year);
    }
    
    public static string GetMonthName(int month, string language)
    {
        if (month < 1 || month > 12) return "";
        
        int index = month - 1;
        
        return language switch
        {
            "ar" => _arabicMonthNames[index],
            "ru" => _russianMonthNames[index],
            "es" => _spanishMonthNames[index],
            "id" => _indonesianMonthNames[index],
            "kk" => _kazakhMonthNames[index],
            _ => _englishMonthNames[index]
        };
    }
    
    public static string FormatHijriDate(DateTime gregorianDate, string language)
    {
        var (day, month, year) = GetHijriDate(gregorianDate);
        string monthName = GetMonthName(month, language);
        
        return $"{day} {monthName} {year}";
    }
    
    public static string FormatFullDate(DateTime gregorianDate, string language)
    {
        string hijriDate = FormatHijriDate(gregorianDate, language);
        string gregorianFormatted = gregorianDate.ToString("d MMMM yyyy", GetCultureInfo(language));
        
        return $"{hijriDate} ({gregorianFormatted})";
    }
    
    private static CultureInfo GetCultureInfo(string language)
    {
        return language switch
        {
            "ar" => new CultureInfo("ar-SA"),
            "ru" => new CultureInfo("ru-RU"),
            "es" => new CultureInfo("es-ES"),
            "id" => new CultureInfo("id-ID"),
            "kk" => new CultureInfo("kk-KZ"),
            _ => new CultureInfo("en-US")
        };
    }
}
