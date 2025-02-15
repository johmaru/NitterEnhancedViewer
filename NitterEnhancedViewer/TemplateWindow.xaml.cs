using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NitterEnhancedViewer.lib.gui;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace NitterEnhancedViewer;

/// <summary>
///   Window template for the application
/// </summary>
public partial class TemplateWindow : UserControl, INotifyPropertyChanged
{

    private SolidColorBrush _dockColor = Brushes.Gray;
    public SolidColorBrush DockColor
    {
        get => _dockColor;
        set
        {
            if (_dockColor != value)
            {
                _dockColor = value;
                OnPropertyChanged(nameof(DockColor));
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    // Dependency property for the custom content
    public static readonly DependencyProperty CustomContentProperty = DependencyProperty.Register(
        nameof(CustomContent), typeof(object), typeof(TemplateWindow));
    
    public object CustomContent
    {
        get => GetValue(CustomContentProperty);
        set => SetValue(CustomContentProperty, value);
    }
    
    public TemplateWindow()
    {
        InitializeComponent();
        Load(Window.GetWindow(this));
    }

    private static async void Load(FrameworkElement? window)
    {
        try
        {
            if (window != null) await WindowController.Init(window);
        }
        catch (Exception e)
        {
            var err = MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (err == MessageBoxResult.OK)
            {
                Environment.Exit(1);
            }
        }
    }
    
    private void Settings_OnClick(object sender, RoutedEventArgs e)
    {
        SettingWindow settingWindow = new SettingWindow(this);
        settingWindow.Show();
    }

    private void Close_OnClick(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
    
    // Event handler for the mouse move event
    public new event MouseEventHandler? MouseMoveEvent;
    private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            MouseMoveEvent?.Invoke(sender, e);
        }
    }
}