using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Media;
using NitterEnhancedViewer.lib.fs;
using NitterEnhancedViewer.lib.gui;
using NitterEnhancedViewer.lib.localization;

namespace NitterEnhancedViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    
    // first method that gets called when the application starts
    private async void Start(object sender, StartupEventArgs e)
    {
        try
        {
            await Init();
        }
        catch (Exception ex)
        {
            var err =  MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (err == MessageBoxResult.OK)
            {
                Environment.Exit(1);
            }
        }
    }

// initialize the application
    private static async ValueTask Init()
    {  
        SettingJson settings =  await JsonController.LoadSettings();

        await JsonController.CheckNullForSettingsData(settings);

        if (!Directory.Exists("data"))
        {
            Directory.CreateDirectory("data");
        }
        
        if (!File.Exists("data/nitterdata.json"))
        {
            await JsonController.SaveNitterData(new List<NitterData>());
        }

        await LanguageController.Initialize();
       
        Current.Resources["DockBrush"] = new SolidColorBrush(settings.DockColor);
        
        if (settings.StartMinimized)
        {
            MainWindow mainWindow = new MainWindow
            {
                WindowState = WindowState.Minimized
            };
            mainWindow.Show();
        } else
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
    
}