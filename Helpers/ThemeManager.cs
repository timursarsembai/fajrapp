using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FajrApp.Services;

namespace FajrApp.Helpers;

public static class ThemeManager
{
    public static void ApplyTheme(Window window, AppSettings settings)
    {
        Color backgroundColor;
        Color textColor;
        Color borderColor;
        Color inputBackgroundColor;
        Color headerBackgroundColor;
        
        if (settings.Theme == AppTheme.Light)
        {
            // Light theme colors
            backgroundColor = Color.FromRgb(245, 245, 245);
            textColor = Color.FromRgb(30, 30, 30);
            borderColor = Color.FromRgb(220, 220, 220);
            inputBackgroundColor = Color.FromRgb(255, 255, 255);
            headerBackgroundColor = Color.FromRgb(235, 235, 235);
        }
        else
        {
            // Dark theme colors
            backgroundColor = Color.FromRgb(30, 30, 30);
            textColor = Color.FromRgb(224, 224, 224);
            borderColor = Color.FromRgb(61, 61, 61);
            inputBackgroundColor = Color.FromRgb(45, 45, 45);
            headerBackgroundColor = Color.FromRgb(37, 37, 37);
        }
        
        // Apply window background
        window.Background = new SolidColorBrush(backgroundColor);
        
        // Apply after window is loaded to ensure visual tree is ready
        if (!window.IsLoaded)
        {
            window.Loaded += (s, e) => ApplyThemeToElements(window, textColor, backgroundColor, borderColor, inputBackgroundColor, headerBackgroundColor);
        }
        else
        {
            ApplyThemeToElements(window, textColor, backgroundColor, borderColor, inputBackgroundColor, headerBackgroundColor);
        }
    }
    
    private static void ApplyThemeToElements(Window window, Color textColor, Color backgroundColor, Color borderColor, Color inputBackgroundColor, Color headerBackgroundColor)
    {
        // Apply to all TextBlocks
        ApplyToAllChildren<TextBlock>(window, tb =>
        {
            // Skip accent colors like #0078D4 (blue), #4CAF50 (green), etc.
            if (tb.Foreground is SolidColorBrush brush)
            {
                var color = brush.Color;
                // Only change neutral colors (whites, grays, blacks)
                if (IsNeutralColor(color))
                {
                    tb.Foreground = new SolidColorBrush(textColor);
                }
            }
        });
        
        // Apply to all TextBoxes
        ApplyToAllChildren<TextBox>(window, textBox =>
        {
            textBox.Background = new SolidColorBrush(inputBackgroundColor);
            textBox.Foreground = new SolidColorBrush(textColor);
            textBox.BorderBrush = new SolidColorBrush(borderColor);
            textBox.CaretBrush = new SolidColorBrush(textColor);
        });
        
        // Apply to all Borders
        ApplyToAllChildren<Border>(window, border =>
        {
            if (border.Background is SolidColorBrush brush)
            {
                var color = brush.Color;
                // Update dark theme backgrounds
                if (color.R == 30 && color.G == 30 && color.B == 30) // #1E1E1E
                {
                    border.Background = new SolidColorBrush(backgroundColor);
                }
                else if (color.R == 37 && color.G == 37 && color.B == 37) // #252525 (header)
                {
                    border.Background = new SolidColorBrush(headerBackgroundColor);
                }
                else if (color.R == 45 && color.G == 45 && color.B == 45) // #2D2D2D (input)
                {
                    border.Background = new SolidColorBrush(inputBackgroundColor);
                }
            }
            
            // Update border colors
            if (border.BorderBrush is SolidColorBrush borderBrush)
            {
                var color = borderBrush.Color;
                if (color.R == 61 && color.G == 61 && color.B == 61) // #3D3D3D
                {
                    border.BorderBrush = new SolidColorBrush(borderColor);
                }
            }
        });
        
        // Apply to Grids with background colors
        ApplyToAllChildren<Grid>(window, grid =>
        {
            if (grid.Background is SolidColorBrush brush)
            {
                var color = brush.Color;
                // Update header backgrounds #252525
                if (color.R == 37 && color.G == 37 && color.B == 37)
                {
                    grid.Background = new SolidColorBrush(headerBackgroundColor);
                }
                // Update main backgrounds #1E1E1E
                else if (color.R == 30 && color.G == 30 && color.B == 30)
                {
                    grid.Background = new SolidColorBrush(backgroundColor);
                }
            }
        });
        
        // Apply to ComboBoxes
        ApplyToAllChildren<ComboBox>(window, comboBox =>
        {
            comboBox.Background = new SolidColorBrush(inputBackgroundColor);
            comboBox.Foreground = new SolidColorBrush(textColor);
            comboBox.BorderBrush = new SolidColorBrush(borderColor);
        });
        
        // Apply to Buttons (but preserve accent colors)
        ApplyToAllChildren<Button>(window, button =>
        {
            if (button.Background is SolidColorBrush brush)
            {
                var color = brush.Color;
                // Only change neutral button backgrounds
                if (color.R == 61 && color.G == 61 && color.B == 61 || // #3D3D3D
                    color == Colors.Transparent)
                {
                    button.Background = new SolidColorBrush(inputBackgroundColor);
                }
            }
            
            if (button.Foreground is SolidColorBrush fgBrush)
            {
                var color = fgBrush.Color;
                if (IsNeutralColor(color))
                {
                    button.Foreground = new SolidColorBrush(textColor);
                }
            }
            
            // Special handling for SettingsWindow tab buttons
            if (button.Name?.EndsWith("TabButton") == true)
            {
                // Set default foreground based on theme (for non-selected state)
                var isSelected = button.Tag?.ToString() == "Selected";
                if (!isSelected)
                {
                    button.Foreground = new SolidColorBrush(textColor);
                }
            }
        });
    }
    
    private static bool IsNeutralColor(Color color)
    {
        // Check if color is grayscale (neutral)
        // Allow small variance for colors like #E0E0E0 or #808080
        int max = Math.Max(Math.Max(color.R, color.G), color.B);
        int min = Math.Min(Math.Min(color.R, color.G), color.B);
        return (max - min) < 30; // Neutral if all RGB values are close
    }
    
    private static void ApplyToAllChildren<T>(DependencyObject parent, Action<T> action) where T : DependencyObject
    {
        if (parent is T target)
        {
            action(target);
        }
        
        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            ApplyToAllChildren(child, action);
        }
    }
}
