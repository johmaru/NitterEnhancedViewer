using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NitterEnhancedViewer.lib.fs;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace NitterEnhancedViewer;


public class FavoritesWindowViewModel : INotifyPropertyChanged
{
    private ObservableCollection<NitterData> _nitterData = new();
    public ObservableCollection<NitterData> NitterData
    {
        get => _nitterData;
        set
        {
            _nitterData = value;
            OnPropertyChanged(nameof(NitterData));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task LoadData()
    {
        var data = await JsonController.LoadNitterData();
        NitterData = new ObservableCollection<NitterData>(data);
    }
}
public partial class FavoritesWindow : FluentWindow
{
    private static Subject<string> _onClicked = new();
    public static IObservable<string> OnClicked => _onClicked.AsObservable();
    
    public static FavoritesWindowViewModel FavViewModel { get; set; }
    public FavoritesWindow()
    {
        InitializeComponent();
        var vm = new FavoritesWindowViewModel();
        FavViewModel = vm;
        DataContext = vm;
        
        Loaded += async (s, e) => 
        {
            await vm.LoadData();
        };
    }

    private void Window_OnMouseMoveEvent(object sender, MouseEventArgs e)
    {
       DragMove();
    }

    private void URL_OnMouseEnter(object sender, MouseEventArgs e)
    {
       ((TextBlock)sender).Foreground = Brushes.Chartreuse;
    }
    
    private void URL_OnMouseLeave(object sender, MouseEventArgs e)
    {
        ((TextBlock)sender).Foreground = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark ? Brushes.White : Brushes.Black;
    }

    private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _onClicked.OnNext(((TextBlock)sender).Text);
    }
}