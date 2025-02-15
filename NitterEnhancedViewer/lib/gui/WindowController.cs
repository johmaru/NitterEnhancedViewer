using System.Windows;
using NitterEnhancedViewer.lib.fs;
using Wpf.Ui.Appearance;

namespace NitterEnhancedViewer.lib.gui;

public abstract class WindowController
{
    public static async ValueTask Init(FrameworkElement window)
    {
        ApplicationThemeManager.Apply(window);
        var settings = await JsonController.LoadSettings();
        ThemeController.SetTheme(settings.Theme); 
    }
}