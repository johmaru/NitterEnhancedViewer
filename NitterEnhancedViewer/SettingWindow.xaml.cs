using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NitterEnhancedViewer.lib.fs;
using NitterEnhancedViewer.lib.gui;
using NitterEnhancedViewer.lib.localization;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;
using PasswordBox = Wpf.Ui.Controls.PasswordBox;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace NitterEnhancedViewer;

public class Language
{
    public string Name { get; set; }
    public string Culture { get; set; }
    
    public Language(string name, string culture)
    {
        Name = name;
        Culture = culture;
    }
}

public class Themes
{
    public string Name { get; set; }
    public ThemeController.Theme Theme { get; set; }
    
    public Themes(string name, ThemeController.Theme theme)
    {
        Name = name;
        Theme = theme;
    }
}

public class DockColors
{
    public string Name { get; set; }
    public Color Color { get; set; }
    
    public DockColors(string name, Color color)
    {
        Name = name;
        Color = color;
    }
}

public class Account
{
    public string Name { get; set; } = string.Empty;
    public string PassWord { get; set; } = string.Empty;
    
    public Account(string name, string password)
    {
        Name = name;
        PassWord = password;
    }
}

public sealed class SettingViewModel : INotifyPropertyChanged
{
    private bool _startMinimized;
    private bool _memoryTab;
    private SettingJson? _settings;
    private Language? _selectedLanguage;
    private Themes? _selectedTheme;
    private DockColors? _selectedColor;
    private string _accountName = string.Empty;
    private string _accountPassword = string.Empty;
    
    
    public ObservableCollection<Language> Languages { get; } = new()
    {
        new Language("English", "en-US"),
        new Language("日本語", "ja-JP"),
    };
    
    public ObservableCollection<Themes> Themes { get; } = new()
    {
        new Themes("Light", ThemeController.Theme.Light),
        new Themes("Dark", ThemeController.Theme.Dark),
    };
    
    public ObservableCollection<DockColors> Colors { get; } = new()
    {
        new DockColors("Gray", System.Windows.Media.Colors.Gray),
        new DockColors("Red", System.Windows.Media.Colors.Red),
        new DockColors("Blue", System.Windows.Media.Colors.Blue),
        new DockColors("Green", System.Windows.Media.Colors.Green),
        new DockColors("Yellow", System.Windows.Media.Colors.Yellow),
        new DockColors("Black", System.Windows.Media.Colors.Black),
        new DockColors("White", System.Windows.Media.Colors.White),
    };

    public ObservableCollection<Account> Accounts { get; } = new();
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public bool StartMinimized
    {
        get => _startMinimized;
        set
        {
            if (_startMinimized != value)
            {
                _startMinimized = value;
                if (_settings != null)
                {
                    _settings.StartMinimized = value;
                    _ = JsonController.SaveSettings(_settings);
                }
                OnPropertyChanged(nameof(StartMinimized));
            }
           
        }
    }

    public bool MemoryTab
    {
        get => _memoryTab;
        set
        {
            if (_memoryTab != value)
            {
                _memoryTab = value;
                if (_settings != null)
                {
                    _settings.MemoryTab = value;
                    _ = JsonController.SaveSettings(_settings);
                }
                OnPropertyChanged(nameof(MemoryTab));
            }
        }
    }
    
    public Language? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (_selectedLanguage != value && value != null)
            {
                _selectedLanguage = value;
                if (_settings != null)
                {
                    _settings.Language = value.Culture;
                    _ = JsonController.SaveSettings(_settings);
                    LanguageController.UpdateLanguage(value.Culture);
                }
                OnPropertyChanged(nameof(SelectedLanguage));
            }
        }
    }
    
    public Themes? SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme != value && value != null)
            {
                _selectedTheme = value;
                if (_settings != null)
                {
                    _settings.Theme = value.Theme;
                    _ = JsonController.SaveSettings(_settings);
                    ThemeController.SetTheme(value.Theme);
                }
                OnPropertyChanged(nameof(SelectedTheme));
            }
        }
    }
    
    public event EventHandler? DockColorChanged;
    public DockColors? SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (_selectedColor != value && value != null)
            {
                _selectedColor = value;
                if (_settings != null)
                {
                    _settings.DockColor = value.Color;
                    _ = JsonController.SaveSettings(_settings);
                    DockColorChanged?.Invoke(this, EventArgs.Empty);
                }
                OnPropertyChanged(nameof(SelectedColor));
            }
        }
    }
    
    public string AccountName
    {
        get => _accountName;
        set
        {
            if (_accountName != value)
            {
                _accountName = value;
                if (_settings != null)
                {
                    _settings.Xusername = value;
                    _ = JsonController.SaveSettings(_settings);
                }
                OnPropertyChanged(nameof(AccountName));
            }
        }
    }
    
    public string AccountPassword
    {
        get => _accountPassword;
        set
        {
            if (_accountPassword != value)
            {
                _accountPassword = value;
                if (_settings != null)
                {
                   _settings.Xpassword = value;
                    _ = JsonController.SaveSettings(_settings);
                }
                OnPropertyChanged(nameof(AccountPassword));
            }
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public SettingViewModel(SettingJson? settings)
    {
        _settings = settings;
        if (settings == null) return;
        _startMinimized = settings.StartMinimized;
        _memoryTab = settings.MemoryTab;
        _selectedLanguage = (Languages.FirstOrDefault(x => x.Culture == settings.Language) ?? Languages.FirstOrDefault()) ?? throw new InvalidOperationException();
        _selectedTheme = (Themes.FirstOrDefault(x => x.Theme == settings.Theme) ?? Themes.FirstOrDefault()) ?? throw new InvalidOperationException();
        _selectedColor = (Colors.FirstOrDefault(x => x.Color == settings.DockColor) ?? Colors.FirstOrDefault()) ?? throw new InvalidOperationException();
        if (settings.Xusername != null) _accountName = settings.Xusername;
    }
}

public partial class SettingWindow : FluentWindow
{
    
   public SettingViewModel ViewModel { get; private set; }
   private TemplateWindow? _templateWindow;
    
    public SettingWindow(TemplateWindow templateWindow)
    {
        InitializeComponent();
        _templateWindow = templateWindow ?? throw new ArgumentNullException(nameof(templateWindow));
        Loaded += SettingWindow_Loaded;
    }

    private async void SettingWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var settings = await JsonController.LoadSettings();
        ViewModel = new SettingViewModel(settings);
            
        ViewModel.DockColorChanged += (sender, args) =>
        {
            if (ViewModel.SelectedColor != null)
            {
              Application.Current.Resources["DockBrush"] = new SolidColorBrush(ViewModel.SelectedColor.Color);
            }
        };
            
        DataContext = ViewModel;
            
        if (settings?.DockColor != null)  
        {
            _templateWindow.DockColor = new SolidColorBrush(settings.DockColor);
        }
    }

    private void Window_OnMouseMoveEvent(object sender, MouseEventArgs e)
    {
        this.DragMove();
    }

    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      if (DataContext is SettingViewModel viewModel)
      {
          viewModel.AccountName = ((TextBox)sender).Text;
      }
    }
/*
    private void TextBoxBase1_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is SettingViewModel viewModel)
        {
            viewModel.AccountPassword = ((PasswordBox)sender).Password;
        }
    }*/

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        LoginXWindow loginXWindow = new LoginXWindow();
        loginXWindow.Show();
    }
}