using System;
using System.Collections.Generic;
using System.Globalization;

namespace FajrApp.Services;

public static class LocalizationService
{
    public static string CurrentLanguage { get; private set; } = "en";
    
    public static event Action? LanguageChanged;
    
    private static readonly Dictionary<string, string> LanguageNames = new()
    {
        { "en", "English" },
        { "es", "Español" },
        { "ar", "العربية" },
        { "ru", "Русский" },
        { "id", "Bahasa Indonesia" },
        { "kk", "Қазақша" }
    };
    
    public static IReadOnlyDictionary<string, string> AvailableLanguages => LanguageNames;
    
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        // English
        { "en", new Dictionary<string, string>
            {
                // Prayer names
                { "Fajr", "Fajr" },
                { "Sunrise", "Sunrise" },
                { "Dhuhr", "Dhuhr" },
                { "Asr", "Asr" },
                { "Maghrib", "Maghrib" },
                { "Isha", "Isha" },
                
                // Widget
                { "In", "in" },
                { "Error", "Error" },
                { "NoData", "No data" },
                
                // Context menu
                { "Settings", "Settings" },
                { "ChangePosition", "Change position" },
                { "AutoStart", "Auto start" },
                { "Refresh", "Refresh" },
                { "SupportProject", "Support the project" },
                { "About", "About" },
                { "Exit", "Exit" },
                
                // Move mode
                { "DragWidget", "⇄ Drag widget | LMB - done" },
                
                // Prayer times window
                { "PrayerTimes", "Prayer Times" },
                { "Today", "Today" },
                
                // Settings window
                { "SettingsTitle", "Settings - FajrApp" },
                { "Location", "Location" },
                { "City", "City" },
                { "SearchCity", "Search city..." },
                { "Search", "Search" },
                { "Coordinates", "Coordinates" },
                { "Latitude", "Latitude" },
                { "Longitude", "Longitude" },
                { "CalculationMethod", "Calculation Method" },
                { "AsrCalculation", "Asr Calculation" },
                { "Standard", "Standard (Shafi, Maliki, Hanbali)" },
                { "Hanafi", "Hanafi" },
                { "TimeOffsets", "Time Offsets (minutes)" },
                { "Language", "Language" },
                { "Save", "Save" },
                { "Cancel", "Cancel" },
                
                // About window
                { "AboutTitle", "About" },
                { "Version", "Version" },
                { "Updated", "Updated" },
                { "Developer", "Developer" },
                { "DeveloperName", "Timur Sarsembai" },
                { "GitHub", "GitHub" },
                { "Close", "Close" },
                
                // Donate window
                { "DonateTitle", "Support the project" },
                { "DonateDescription", "If you like FajrApp, you can support its development through one of the following platforms:" },
                { "ThankYou", "Thank you for your support! 🙏" },
                
                // Notifications
                { "Notifications", "Notifications" },
                { "EnableNotifications", "Enable notifications" },
                { "NotificationSound", "Notification sound" },
                { "SoundNone", "None" },
                { "SoundSystem", "System sound" },
                { "SoundAzan", "Azan" },
                { "AzanHint", "To use Azan, add azan.mp3 and azan_fajr.mp3 files to the Sounds folder" },
                { "OpenSoundsFolder", "Open Sounds folder" },
                { "PrayerTime", "Prayer time" },
                
                // Calculation methods
                { "MethodMuslimWorldLeague", "Muslim World League" },
                { "MethodISNA", "ISNA (North America)" },
                { "MethodEgypt", "Egyptian General Authority" },
                { "MethodMakkah", "Umm Al-Qura (Makkah)" },
                { "MethodKarachi", "University of Karachi" },
                { "MethodTehran", "Tehran University" },
                { "MethodJafari", "Shia Ithna-Ashari (Jafari)" },
                { "MethodGulf", "Gulf Region" },
                { "MethodKuwait", "Kuwait" },
                { "MethodQatar", "Qatar" },
                { "MethodSingapore", "MUIS (Singapore)" },
                { "MethodFrance", "France (UOIF)" },
                { "MethodTurkey", "Diyanet (Turkey)" },
                { "MethodRussia", "Russia" },
                { "MethodDubai", "Dubai" },
                { "MethodMoonsighting", "Moonsighting Committee" }
            }
        },
        
        // Spanish
        { "es", new Dictionary<string, string>
            {
                { "Fajr", "Fajr" },
                { "Sunrise", "Amanecer" },
                { "Dhuhr", "Dhuhr" },
                { "Asr", "Asr" },
                { "Maghrib", "Maghrib" },
                { "Isha", "Isha" },
                
                { "In", "en" },
                { "Error", "Error" },
                { "NoData", "Sin datos" },
                
                { "Settings", "Configuración" },
                { "ChangePosition", "Cambiar posición" },
                { "AutoStart", "Inicio automático" },
                { "Refresh", "Actualizar" },
                { "SupportProject", "Apoyar el proyecto" },
                { "About", "Acerca de" },
                { "Exit", "Salir" },
                
                { "DragWidget", "⇄ Arrastre el widget | Clic - listo" },
                
                { "PrayerTimes", "Horarios de Oración" },
                { "Today", "Hoy" },
                
                { "SettingsTitle", "Configuración - FajrApp" },
                { "Location", "Ubicación" },
                { "City", "Ciudad" },
                { "SearchCity", "Buscar ciudad..." },
                { "Search", "Buscar" },
                { "Coordinates", "Coordenadas" },
                { "Latitude", "Latitud" },
                { "Longitude", "Longitud" },
                { "CalculationMethod", "Método de Cálculo" },
                { "AsrCalculation", "Cálculo de Asr" },
                { "Standard", "Estándar (Shafi, Maliki, Hanbali)" },
                { "Hanafi", "Hanafi" },
                { "TimeOffsets", "Ajustes de Tiempo (minutos)" },
                { "Language", "Idioma" },
                { "Save", "Guardar" },
                { "Cancel", "Cancelar" },
                
                { "AboutTitle", "Acerca de" },
                { "Version", "Versión" },
                { "Updated", "Actualizado" },
                { "Developer", "Desarrollador" },
                { "DeveloperName", "Timur Sarsembai" },
                { "GitHub", "GitHub" },
                { "Close", "Cerrar" },
                
                // Donate window
                { "DonateTitle", "Apoyar el proyecto" },
                { "DonateDescription", "Si te gusta FajrApp, puedes apoyar su desarrollo a través de una de las siguientes plataformas:" },
                { "ThankYou", "¡Gracias por tu apoyo! 🙏" },
                
                // Notifications
                { "Notifications", "Notificaciones" },
                { "EnableNotifications", "Habilitar notificaciones" },
                { "NotificationSound", "Sonido de notificación" },
                { "SoundNone", "Ninguno" },
                { "SoundSystem", "Sonido del sistema" },
                { "SoundAzan", "Adhan" },
                { "AzanHint", "Para usar Adhan, agregue archivos azan.mp3 y azan_fajr.mp3 a la carpeta Sounds" },
                { "OpenSoundsFolder", "Abrir carpeta Sounds" },
                { "PrayerTime", "Hora de oración" },
                
                { "MethodMuslimWorldLeague", "Liga Mundial Musulmana" },
                { "MethodISNA", "ISNA (Norteamérica)" },
                { "MethodEgypt", "Autoridad General de Egipto" },
                { "MethodMakkah", "Umm Al-Qura (La Meca)" },
                { "MethodKarachi", "Universidad de Karachi" },
                { "MethodTehran", "Universidad de Teherán" },
                { "MethodJafari", "Shia Ithna-Ashari (Jafari)" },
                { "MethodGulf", "Región del Golfo" },
                { "MethodKuwait", "Kuwait" },
                { "MethodQatar", "Qatar" },
                { "MethodSingapore", "MUIS (Singapur)" },
                { "MethodFrance", "Francia (UOIF)" },
                { "MethodTurkey", "Diyanet (Turquía)" },
                { "MethodRussia", "Rusia" },
                { "MethodDubai", "Dubai" },
                { "MethodMoonsighting", "Comité Moonsighting" }
            }
        },
        
        // Arabic
        { "ar", new Dictionary<string, string>
            {
                { "Fajr", "الفجر" },
                { "Sunrise", "الشروق" },
                { "Dhuhr", "الظهر" },
                { "Asr", "العصر" },
                { "Maghrib", "المغرب" },
                { "Isha", "العشاء" },
                
                { "In", "بعد" },
                { "Error", "خطأ" },
                { "NoData", "لا توجد بيانات" },
                
                { "Settings", "الإعدادات" },
                { "ChangePosition", "تغيير الموضع" },
                { "AutoStart", "بدء تلقائي" },
                { "Refresh", "تحديث" },
                { "SupportProject", "دعم المشروع" },
                { "About", "حول" },
                { "Exit", "خروج" },
                
                { "DragWidget", "⇄ اسحب الأداة | انقر - تم" },
                
                { "PrayerTimes", "مواقيت الصلاة" },
                { "Today", "اليوم" },
                
                { "SettingsTitle", "الإعدادات - FajrApp" },
                { "Location", "الموقع" },
                { "City", "المدينة" },
                { "SearchCity", "ابحث عن مدينة..." },
                { "Search", "بحث" },
                { "Coordinates", "الإحداثيات" },
                { "Latitude", "خط العرض" },
                { "Longitude", "خط الطول" },
                { "CalculationMethod", "طريقة الحساب" },
                { "AsrCalculation", "حساب العصر" },
                { "Standard", "قياسي (شافعي، مالكي، حنبلي)" },
                { "Hanafi", "حنفي" },
                { "TimeOffsets", "تعديلات الوقت (دقائق)" },
                { "Language", "اللغة" },
                { "Save", "حفظ" },
                { "Cancel", "إلغاء" },
                
                { "AboutTitle", "حول" },
                { "Version", "الإصدار" },
                { "Updated", "تم التحديث" },
                { "Developer", "المطور" },
                { "DeveloperName", "تيمور سارسمباي" },
                { "GitHub", "GitHub" },
                { "Close", "إغلاق" },
                
                // Donate window
                { "DonateTitle", "دعم المشروع" },
                { "DonateDescription", "إذا أعجبك FajrApp، يمكنك دعم تطويره من خلال إحدى المنصات التالية:" },
                { "ThankYou", "شكراً لدعمك! 🙏" },
                
                // Notifications
                { "Notifications", "الإشعارات" },
                { "EnableNotifications", "تفعيل الإشعارات" },
                { "NotificationSound", "صوت الإشعار" },
                { "SoundNone", "بدون صوت" },
                { "SoundSystem", "صوت النظام" },
                { "SoundAzan", "الأذان" },
                { "AzanHint", "لاستخدام الأذان، أضف ملفات azan.mp3 و azan_fajr.mp3 إلى مجلد Sounds" },
                { "OpenSoundsFolder", "فتح مجلد Sounds" },
                { "PrayerTime", "وقت الصلاة" },
                
                { "MethodMuslimWorldLeague", "رابطة العالم الإسلامي" },
                { "MethodISNA", "ISNA (أمريكا الشمالية)" },
                { "MethodEgypt", "الهيئة المصرية العامة" },
                { "MethodMakkah", "أم القرى (مكة)" },
                { "MethodKarachi", "جامعة كراتشي" },
                { "MethodTehran", "جامعة طهران" },
                { "MethodJafari", "الشيعة الإثني عشرية (جعفري)" },
                { "MethodGulf", "منطقة الخليج" },
                { "MethodKuwait", "الكويت" },
                { "MethodQatar", "قطر" },
                { "MethodSingapore", "MUIS (سنغافورة)" },
                { "MethodFrance", "فرنسا (UOIF)" },
                { "MethodTurkey", "ديانت (تركيا)" },
                { "MethodRussia", "روسيا" },
                { "MethodDubai", "دبي" },
                { "MethodMoonsighting", "لجنة رؤية الهلال" }
            }
        },
        
        // Russian
        { "ru", new Dictionary<string, string>
            {
                { "Fajr", "Фаджр" },
                { "Sunrise", "Восход" },
                { "Dhuhr", "Зухр" },
                { "Asr", "Аср" },
                { "Maghrib", "Магриб" },
                { "Isha", "Иша" },
                
                { "In", "через" },
                { "Error", "Ошибка" },
                { "NoData", "Нет данных" },
                
                { "Settings", "Настройки" },
                { "ChangePosition", "Изменить положение" },
                { "AutoStart", "Автозапуск" },
                { "Refresh", "Обновить" },
                { "SupportProject", "Помогите проекту" },
                { "About", "О приложении" },
                { "Exit", "Выход" },
                
                { "DragWidget", "⇄ Перетащите виджет | ЛКМ - готово" },
                
                { "PrayerTimes", "Времена молитв" },
                { "Today", "Сегодня" },
                
                { "SettingsTitle", "Настройки - FajrApp" },
                { "Location", "Местоположение" },
                { "City", "Город" },
                { "SearchCity", "Поиск города..." },
                { "Search", "Найти" },
                { "Coordinates", "Координаты" },
                { "Latitude", "Широта" },
                { "Longitude", "Долгота" },
                { "CalculationMethod", "Метод расчёта" },
                { "AsrCalculation", "Расчёт Асра" },
                { "Standard", "Стандартный (Шафии, Малики, Ханбали)" },
                { "Hanafi", "Ханафи" },
                { "TimeOffsets", "Смещение времени (минуты)" },
                { "Language", "Язык" },
                { "Save", "Сохранить" },
                { "Cancel", "Отмена" },
                
                { "AboutTitle", "О приложении" },
                { "Version", "Версия" },
                { "Updated", "Обновлено" },
                { "Developer", "Разработчик" },
                { "DeveloperName", "Тимур Сарсембай" },
                { "GitHub", "GitHub" },
                { "Close", "Закрыть" },
                
                // Donate window
                { "DonateTitle", "Помогите проекту" },
                { "DonateDescription", "Если вам нравится FajrApp, вы можете поддержать его развитие через одну из следующих платформ:" },
                { "ThankYou", "Спасибо за вашу поддержку! 🙏" },
                
                // Notifications
                { "Notifications", "Уведомления" },
                { "EnableNotifications", "Включить уведомления" },
                { "NotificationSound", "Звук уведомления" },
                { "SoundNone", "Без звука" },
                { "SoundSystem", "Системный звук" },
                { "SoundAzan", "Азан" },
                { "AzanHint", "Для использования азана добавьте файлы azan.mp3 и azan_fajr.mp3 в папку Sounds" },
                { "OpenSoundsFolder", "Открыть папку Sounds" },
                { "PrayerTime", "Время намаза" },
                
                { "MethodMuslimWorldLeague", "Всемирная исламская лига" },
                { "MethodISNA", "ISNA (Северная Америка)" },
                { "MethodEgypt", "Египетское управление" },
                { "MethodMakkah", "Умм аль-Кура (Мекка)" },
                { "MethodKarachi", "Университет Карачи" },
                { "MethodTehran", "Университет Тегерана" },
                { "MethodJafari", "Шиа Итна-Ашари (Джафари)" },
                { "MethodGulf", "Персидский залив" },
                { "MethodKuwait", "Кувейт" },
                { "MethodQatar", "Катар" },
                { "MethodSingapore", "MUIS (Сингапур)" },
                { "MethodFrance", "Франция (UOIF)" },
                { "MethodTurkey", "Диянет (Турция)" },
                { "MethodRussia", "Россия" },
                { "MethodDubai", "Дубай" },
                { "MethodMoonsighting", "Moonsighting Committee" }
            }
        },
        
        // Indonesian
        { "id", new Dictionary<string, string>
            {
                { "Fajr", "Subuh" },
                { "Sunrise", "Terbit" },
                { "Dhuhr", "Dzuhur" },
                { "Asr", "Ashar" },
                { "Maghrib", "Maghrib" },
                { "Isha", "Isya" },
                
                { "In", "dalam" },
                { "Error", "Kesalahan" },
                { "NoData", "Tidak ada data" },
                
                { "Settings", "Pengaturan" },
                { "ChangePosition", "Ubah posisi" },
                { "AutoStart", "Mulai otomatis" },
                { "Refresh", "Segarkan" },
                { "SupportProject", "Dukung proyek" },
                { "About", "Tentang" },
                { "Exit", "Keluar" },
                
                { "DragWidget", "⇄ Seret widget | Klik - selesai" },
                
                { "PrayerTimes", "Jadwal Sholat" },
                { "Today", "Hari ini" },
                
                { "SettingsTitle", "Pengaturan - FajrApp" },
                { "Location", "Lokasi" },
                { "City", "Kota" },
                { "SearchCity", "Cari kota..." },
                { "Search", "Cari" },
                { "Coordinates", "Koordinat" },
                { "Latitude", "Lintang" },
                { "Longitude", "Bujur" },
                { "CalculationMethod", "Metode Perhitungan" },
                { "AsrCalculation", "Perhitungan Ashar" },
                { "Standard", "Standar (Syafi'i, Maliki, Hanbali)" },
                { "Hanafi", "Hanafi" },
                { "TimeOffsets", "Penyesuaian Waktu (menit)" },
                { "Language", "Bahasa" },
                { "Save", "Simpan" },
                { "Cancel", "Batal" },
                
                { "AboutTitle", "Tentang" },
                { "Version", "Versi" },
                { "Updated", "Diperbarui" },
                { "Developer", "Pengembang" },
                { "DeveloperName", "Timur Sarsembai" },
                { "GitHub", "GitHub" },
                { "Close", "Tutup" },
                
                // Donate window
                { "DonateTitle", "Dukung proyek" },
                { "DonateDescription", "Jika Anda menyukai FajrApp, Anda dapat mendukung pengembangannya melalui salah satu platform berikut:" },
                { "ThankYou", "Terima kasih atas dukungan Anda! 🙏" },
                
                // Notifications
                { "Notifications", "Notifikasi" },
                { "EnableNotifications", "Aktifkan notifikasi" },
                { "NotificationSound", "Suara notifikasi" },
                { "SoundNone", "Tidak ada" },
                { "SoundSystem", "Suara sistem" },
                { "SoundAzan", "Azan" },
                { "AzanHint", "Untuk menggunakan Azan, tambahkan file azan.mp3 dan azan_fajr.mp3 ke folder Sounds" },
                { "OpenSoundsFolder", "Buka folder Sounds" },
                { "PrayerTime", "Waktu sholat" },
                
                { "MethodMuslimWorldLeague", "Liga Muslim Dunia" },
                { "MethodISNA", "ISNA (Amerika Utara)" },
                { "MethodEgypt", "Otoritas Umum Mesir" },
                { "MethodMakkah", "Umm Al-Qura (Mekkah)" },
                { "MethodKarachi", "Universitas Karachi" },
                { "MethodTehran", "Universitas Teheran" },
                { "MethodJafari", "Syiah Itsna Asyari (Jafari)" },
                { "MethodGulf", "Wilayah Teluk" },
                { "MethodKuwait", "Kuwait" },
                { "MethodQatar", "Qatar" },
                { "MethodSingapore", "MUIS (Singapura)" },
                { "MethodFrance", "Prancis (UOIF)" },
                { "MethodTurkey", "Diyanet (Turki)" },
                { "MethodRussia", "Rusia" },
                { "MethodDubai", "Dubai" },
                { "MethodMoonsighting", "Komite Moonsighting" }
            }
        },
        
        // Kazakh
        { "kk", new Dictionary<string, string>
            {
                { "Fajr", "Таң намазы" },
                { "Sunrise", "Күн шығысы" },
                { "Dhuhr", "Бесін намазы" },
                { "Asr", "Екінті намазы" },
                { "Maghrib", "Ақшам намазы" },
                { "Isha", "Құптан намазы" },
                
                { "In", "кейін" },
                { "Error", "Қате" },
                { "NoData", "Деректер жоқ" },
                
                { "Settings", "Баптаулар" },
                { "ChangePosition", "Орнын өзгерту" },
                { "AutoStart", "Авто іске қосу" },
                { "Refresh", "Жаңарту" },
                { "SupportProject", "Жобаны қолдаңыз" },
                { "About", "Бағдарлама туралы" },
                { "Exit", "Шығу" },
                
                { "DragWidget", "⇄ Виджетті сүйреңіз | Басу - дайын" },
                
                { "PrayerTimes", "Намаз уақыттары" },
                { "Today", "Бүгін" },
                
                { "SettingsTitle", "Баптаулар - FajrApp" },
                { "Location", "Орналасқан жері" },
                { "City", "Қала" },
                { "SearchCity", "Қала іздеу..." },
                { "Search", "Іздеу" },
                { "Coordinates", "Координаттар" },
                { "Latitude", "Ендік" },
                { "Longitude", "Бойлық" },
                { "CalculationMethod", "Есептеу әдісі" },
                { "AsrCalculation", "Екінті есептеу" },
                { "Standard", "Стандартты (Шафиғи, Мәлики, Ханбали)" },
                { "Hanafi", "Ханафи" },
                { "TimeOffsets", "Уақыт түзету (минут)" },
                { "Language", "Тіл" },
                { "Save", "Сақтау" },
                { "Cancel", "Болдырмау" },
                
                { "AboutTitle", "Бағдарлама туралы" },
                { "Version", "Нұсқа" },
                { "Updated", "Жаңартылды" },
                { "Developer", "Әзірлеуші" },
                { "DeveloperName", "Тимур Сарсембай" },
                { "GitHub", "GitHub" },
                { "Close", "Жабу" },
                
                // Donate window
                { "DonateTitle", "Жобаны қолдаңыз" },
                { "DonateDescription", "Егер FajrApp ұнаса, оны келесі платформалар арқылы қолдай аласыз:" },
                { "ThankYou", "Қолдағаныңыз үшін рахмет! 🙏" },
                
                // Notifications
                { "Notifications", "Хабарландырулар" },
                { "EnableNotifications", "Хабарландыруларды қосу" },
                { "NotificationSound", "Хабарландыру дыбысы" },
                { "SoundNone", "Дыбыссыз" },
                { "SoundSystem", "Жүйелік дыбыс" },
                { "SoundAzan", "Азан" },
                { "AzanHint", "Азанды пайдалану үшін Sounds папкасына azan.mp3 және azan_fajr.mp3 файлдарын қосыңыз" },
                { "OpenSoundsFolder", "Sounds папкасын ашу" },
                { "PrayerTime", "Намаз уақыты" },
                
                { "MethodMuslimWorldLeague", "Бүкіләлемдік ислам лигасы" },
                { "MethodISNA", "ISNA (Солтүстік Америка)" },
                { "MethodEgypt", "Мысыр басқармасы" },
                { "MethodMakkah", "Умм әл-Құра (Мекке)" },
                { "MethodKarachi", "Карачи университеті" },
                { "MethodTehran", "Тегеран университеті" },
                { "MethodJafari", "Шия Итна-Ашари (Джафари)" },
                { "MethodGulf", "Парсы шығанағы" },
                { "MethodKuwait", "Кувейт" },
                { "MethodQatar", "Катар" },
                { "MethodSingapore", "MUIS (Сингапур)" },
                { "MethodFrance", "Франция (UOIF)" },
                { "MethodTurkey", "Диянет (Түркия)" },
                { "MethodRussia", "Ресей" },
                { "MethodDubai", "Дубай" },
                { "MethodMoonsighting", "Moonsighting Committee" }
            }
        }
    };
    
    public static void Initialize()
    {
        var settings = SettingsService.Load();
        
        if (string.IsNullOrEmpty(settings.Language))
        {
            // Auto-detect system language
            var systemLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
            
            CurrentLanguage = systemLang switch
            {
                "es" => "es",
                "ar" => "ar",
                "ru" => "ru",
                "id" => "id",
                "kk" => "kk",
                _ => "en"
            };
            
            settings.Language = CurrentLanguage;
            SettingsService.Save(settings);
        }
        else
        {
            CurrentLanguage = settings.Language;
        }
    }
    
    public static void SetLanguage(string langCode)
    {
        if (Translations.ContainsKey(langCode))
        {
            CurrentLanguage = langCode;
            
            var settings = SettingsService.Load();
            settings.Language = langCode;
            SettingsService.Save(settings);
            
            LanguageChanged?.Invoke();
        }
    }
    
    public static string Get(string key)
    {
        if (Translations.TryGetValue(CurrentLanguage, out var langDict))
        {
            if (langDict.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        
        // Fallback to English
        if (Translations.TryGetValue("en", out var enDict))
        {
            if (enDict.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        
        return key; // Return key if not found
    }
    
    // Shorthand
    public static string T(string key) => Get(key);
}
