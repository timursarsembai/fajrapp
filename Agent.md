# Agent Notes (FajrApp)

## Project Snapshot
- Stack: C# + .NET 8 + WPF (`net8.0-windows`).
- App type: taskbar prayer-time widget for Windows.
- Entry: `App.xaml` -> `MainWindow`.
- Build packaging present: portable, installer scripts, `FajrApp.Package` (MSIX).

## High-Level Structure
- `MainWindow.xaml/.cs`: widget UI, timer loop, next prayer countdown, popup windows, prayer-time trigger.
- `PrayerTimesWindow.xaml/.cs`: table with all prayer times, city, hijri date, quick settings access.
- `NotificationWindow.xaml/.cs`: popup notification on prayer time.
- `Services/PrayerService.cs`: fetches timings (Aladhan API), applies offsets, computes current/next prayer.
- `Services/NotificationService.cs`: sound playback (`MediaPlayer`), azan files, active playback state.
- `Services/SettingsService.cs`: persisted app settings/cache in `%LocalAppData%/FajrApp/settings.json`.
- `Services/LocalizationService.cs`: in-code dictionary translations (`en/es/ar/ru/id/kk`).
- `Helpers/*`: taskbar positioning, autostart registry, virtual desktop support, mouse hook.

## Prayer Time + Azan Flow
1. `MainWindow` updates every second (`DispatcherTimer`).
2. `CheckAndNotifyPrayerTime(...)` detects prayer start window (`0..30s`).
3. Calls `NotificationService.NotifyPrayerTime(...)` with prayer key (`Fajr/Dhuhr/Asr/Maghrib/Isha`).
4. `NotificationService` sets `ActivePrayerKey`, plays selected sound, exposes playback state:
   - `IsPlaying`
   - `ActivePrayerKey`
   - `IsPlayingChanged` event

## Table Stop Button Behavior
- `PrayerTimesWindow` has dedicated stop buttons per prayer row:
  - `FajrStopButton`, `DhuhrStopButton`, `AsrStopButton`, `MaghribStopButton`, `IshaStopButton`
- `UpdateStopButtons()` shows only one button for currently playing azan (`NotificationService.ActivePrayerKey`).
- `StopButton_Click` calls `NotificationService.StopSound()`.

## Important Implementation Notes
- `PrayerService.GetCurrentPrayerName(...)` returns localized text, but `PrayerTimesWindow.LoadTimes()` compares against hardcoded localized literals; this is brittle for multi-language behavior.
- There is an unusual file named `nul` in repo root; `rg` can throw `os error 1` because of it.

## Change Implemented In This Task
- File: `PrayerTimesWindow.xaml.cs`
- Updated deactivation-close logic so the prayer table stays open while azan is playing:
  - Before: window always closed on deactivation (unless settings requested).
  - Now: it does **not** auto-close when `NotificationService.IsPlaying == true`.
- Reason: user can reliably press `Stop` on the active prayer row during azan playback.
