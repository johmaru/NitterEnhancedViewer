using Wpf.Ui.Appearance;

namespace NitterEnhancedViewer.lib.gui;

public abstract class ThemeController
{
    public enum Theme
    {
        Light,
        Dark
    }
    
    public static void SetTheme(Theme theme)
    {
        ApplicationThemeManager.Apply(theme == Theme.Light ? ApplicationTheme.Light:ApplicationTheme.Dark );
    }
}