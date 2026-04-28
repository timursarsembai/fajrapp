using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;

namespace FajrApp.Services;

public enum NotificationSoundType
{
    None = 0,
    System = 1,
    Azan = 2
}

public static class NotificationService
{
    private static MediaPlayer? _mediaPlayer;
    private static bool _isPlaying;
    
    public static bool IsPlaying => _isPlaying;
    public static string? ActivePrayerKey { get; private set; }
    
    public static event EventHandler? IsPlayingChanged;
    
    private static readonly string SoundsFolder = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Sounds"
    );
    
    private static readonly string FajrAzanPath = Path.Combine(SoundsFolder, "azan_fajr.mp3");
    private static readonly string RegularAzanPath = Path.Combine(SoundsFolder, "azan.mp3");
    
    /// <summary>
    /// Shows notification and plays sound for prayer time
    /// </summary>
    public static void NotifyPrayerTime(string prayerName, string prayerTime, NotificationSoundType soundType, bool isFajr, string prayerKey)
    {
        ActivePrayerKey = prayerKey;
        
        // Show notification popup
        ShowNotificationPopup(prayerName, prayerTime);
        
        // Play sound
        PlaySound(soundType, isFajr);
    }
    
    /// <summary>
    /// Shows a simple notification popup window
    /// </summary>
    private static void ShowNotificationPopup(string prayerName, string prayerTime)
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var notification = new NotificationWindow(prayerName, prayerTime);
                notification.Show();
            });
        }
        catch
        {
            // Notification popup may fail silently
        }
    }
    
    /// <summary>
    /// Plays the notification sound based on settings
    /// </summary>
    public static void PlaySound(NotificationSoundType soundType, bool isFajr)
    {
        if (_isPlaying || soundType == NotificationSoundType.None)
        {
            if (soundType == NotificationSoundType.None)
            {
                ActivePrayerKey = null;
                IsPlayingChanged?.Invoke(null, EventArgs.Empty);
            }
            return;
        }
            
        try
        {
            switch (soundType)
            {
                case NotificationSoundType.System:
                    SystemSounds.Exclamation.Play();
                    ActivePrayerKey = null; // System sound doesn't stay playing
                    IsPlayingChanged?.Invoke(null, EventArgs.Empty);
                    break;
                    
                case NotificationSoundType.Azan:
                    PlayAzan(isFajr);
                    break;
            }
        }
        catch
        {
            // Fallback to system sound if azan fails
            try { SystemSounds.Exclamation.Play(); } catch { }
            ActivePrayerKey = null;
            IsPlayingChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Plays the appropriate azan sound
    /// </summary>
    private static void PlayAzan(bool isFajr)
    {
        string azanPath = isFajr ? FajrAzanPath : RegularAzanPath;
        
        // If Fajr azan doesn't exist, fall back to regular azan
        if (isFajr && !File.Exists(FajrAzanPath))
        {
            azanPath = RegularAzanPath;
        }
        
        if (!File.Exists(azanPath))
        {
            // No azan file found, play system sound
            SystemSounds.Exclamation.Play();
            ActivePrayerKey = null;
            IsPlayingChanged?.Invoke(null, EventArgs.Empty);
            return;
        }
        
        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                StopSound();
                
                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.MediaEnded += (s, e) =>
                {
                    _isPlaying = false;
                    ActivePrayerKey = null;
                    IsPlayingChanged?.Invoke(null, EventArgs.Empty);
                    _mediaPlayer?.Close();
                };
                _mediaPlayer.MediaFailed += (s, e) =>
                {
                    _isPlaying = false;
                    ActivePrayerKey = null;
                    IsPlayingChanged?.Invoke(null, EventArgs.Empty);
                    _mediaPlayer?.Close();
                };
                
                _mediaPlayer.Open(new Uri(azanPath));
                _mediaPlayer.Volume = 1.0;
                _mediaPlayer.Play();
                _isPlaying = true;
                IsPlayingChanged?.Invoke(null, EventArgs.Empty);
            }
            catch
            {
                _isPlaying = false;
                ActivePrayerKey = null;
                IsPlayingChanged?.Invoke(null, EventArgs.Empty);
            }
        });
    }
    
    /// <summary>
    /// Stops currently playing sound
    /// </summary>
    public static void StopSound()
    {
        try
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Close();
                _mediaPlayer = null;
            }
            _isPlaying = false;
            ActivePrayerKey = null;
            IsPlayingChanged?.Invoke(null, EventArgs.Empty);
        }
        catch { }
    }
    
    /// <summary>
    /// Checks if azan files exist
    /// </summary>
    public static bool AzanFilesExist()
    {
        return File.Exists(RegularAzanPath) || File.Exists(FajrAzanPath);
    }
    
    /// <summary>
    /// Gets the sounds folder path
    /// </summary>
    public static string GetSoundsFolder() => SoundsFolder;
    
    /// <summary>
    /// Creates the sounds folder if it doesn't exist
    /// </summary>
    public static void EnsureSoundsFolderExists()
    {
        if (!Directory.Exists(SoundsFolder))
        {
            Directory.CreateDirectory(SoundsFolder);
        }
    }
}
