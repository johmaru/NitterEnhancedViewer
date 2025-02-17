using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NitterEnhancedViewer.lib.fs;
using NitterEnhancedViewer.lib.gui;
using NitterEnhancedViewer.lib.localization;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace NitterEnhancedViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

public class MainWindowViewModel : INotifyPropertyChanged
{
    private bool _isOpen;
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen != value)
            {
                _isOpen = value;
                OnPropertyChanged(nameof(IsOpen));
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
public partial class MainWindow : FluentWindow
{
    public MainWindowViewModel ViewModel { get;}
    
    private static readonly string _nitterUrl = "https://nitter.net/";
    
    private WebView2? _webView;
    
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainWindowViewModel();
        DataContext = ViewModel;
        Activate();
    }
    
    private static T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(obj, i);
            if (child is T matchingChild)
                return matchingChild;
          
            T? childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null) return childOfChild;
        }
        return null;
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            SettingsMain settingsMain = new SettingsMain();
            this.Width = settingsMain.MainWindowWidth;
            this.Height = settingsMain.MainWindowHeight;

            var json = await JsonController.LoadSettings();
            
            _webView = FindVisualChild<WebView2>(Window);
            if (_webView == null) return;
            _webView.Source = new Uri(_nitterUrl);
            _webView.NavigationCompleted += WebViewOnNavigationCompleted;
            _webView.SourceChanged += WebViewOnSourceChanged;
           
            
            if (!string.IsNullOrEmpty(json.Xusername) || !string.IsNullOrEmpty(json.Xpassword)) return;
            
        }
        catch (Exception exception)
        {
            var err = MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (err == MessageBoxResult.OK)
            {
                Environment.Exit(1);
            }
        }
    }

    private async void WebViewOnSourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
        if (_webView != null && _webView.Source.AbsoluteUri.Contains("nitter.net"))
        {
            var settings = await JsonController.LoadSettings();
            await _webView.ExecuteScriptAsync($@"
            const navItem = document.querySelector('.inner-nav');
            if (navItem) {{
                const newNavItem = document.createElement('div');
                newNavItem.className = 'nav-item';
                const newLink = document.createElement('a');
                newLink.href = '#';
                newLink.textContent = '{LanguageController.GetLocalizedValue<string>("S_Profile")}';
                newLink.onclick = (e) => {{
                    e.preventDefault();
                    window.chrome.webview.postMessage('profile');
                }};

                const TweetItem = document.createElement('div');
                TweetItem.className = 'nav-item';
                const TweetLink = document.createElement('a');
                TweetLink.href = '#';
                TweetLink.textContent = '{LanguageController.GetLocalizedValue<string>("S_Tweet")}';
                TweetLink.onclick = (e) => {{
                    e.preventDefault();
                    window.chrome.webview.postMessage('tweet');
                }};

                newNavItem.appendChild(newLink);
                navItem.appendChild(newNavItem);

                TweetItem.appendChild(TweetLink);
                navItem.appendChild(TweetItem);
            }}
        ");
            
        }
    }
    
    private async Task HandleWebMessage(string? message, List<NitterData> data, SettingJson  settings)
    {
        if (message == null) return;

        if (message == "profile")
        {
            _webView.Source = new Uri($"https://nitter.net/{settings.Xusername}");
            return;
        }
        
        if (message == "tweet")
        {
            TweetWindow tweetWindow = new TweetWindow();
            tweetWindow.Show();
            tweetWindow.Activate();
            return;
        }

        if (message.StartsWith("save:"))
        {
            var tweetId = message.Split(':')[1];
            var existingData = data.FirstOrDefault(x => x.Url == tweetId);

            if (existingData != null)
            {
                await JsonController.RemoveNitterData(existingData);
                data.Remove(existingData);
            }
            else
            {
                var newData = new NitterData
                {
                    Url = tweetId,
                    FavoriteTime = DateTime.Now,
                    IsFavorite = true
                };
                await JsonController.AppendNitterData(newData);
                data.Add(newData);
            }
        }
    }

    private async void WebViewOnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess) return;
        
      if (_webView != null && _webView.Source.AbsoluteUri.Contains("nitter.net"))
      {
          var settings = await JsonController.LoadSettings();
          var data = await JsonController.LoadNitterData();
          
         await _webView.ExecuteScriptAsync($@"
        const timelineItems = document.querySelectorAll('.timeline-item');
        const favoriteUrls = {JsonSerializer.Serialize(data.Select(x => x.Url))};

        const style = document.createElement('style');
        style.textContent = `
        .tweet-link \{{
            pointer-events: auto;
            display: inline-block;
        \}}
        .tweet-stats \{{
            display: flex;
            align-items: center;
            flex-wrap: wrap;
        \}}
        .tweet-stat \{{
            display: inline-flex;
            align-items: center;
            margin-right: 8px;
        \}}
        .save-button \{{
            position: relative;
            z-index: 9999;
            pointer-events: auto;
        \}}
      `;
        document.head.appendChild(style);

        timelineItems.forEach(item => {{
            const tweetLink = item.querySelector('a.tweet-link');
            if (!tweetLink) return;

            const tweetHref = tweetLink.getAttribute('href');
            const tweetStats = item.querySelector('.tweet-stats');
            if (!tweetStats) return;

     
            if (tweetStats.querySelector('.save-button')) return;

            
            const saveSpan = document.createElement('span');
            saveSpan.className = 'tweet-stat save-button';

            const iconWrapper = document.createElement('div');
            iconWrapper.className = 'icon-container';

            const saveIcon = document.createElement('span');
            const isSaved = favoriteUrls.includes(tweetHref);
            saveIcon.className = 'icon-heart';
            saveIcon.title = isSaved ? '保存済み' : '保存';
            saveIcon.style.color = isSaved ? '#e0245e' : '';

            iconWrapper.onclick = (e) => {{
                e.preventDefault();
                e.stopPropagation();
                window.chrome.webview.postMessage(`save:${{tweetHref}}`);
                const newIsSaved = saveIcon.style.color === '';
                saveIcon.style.color = newIsSaved ? '#e0245e' : '';
                saveIcon.title = newIsSaved ? '保存済み' : '保存';
            }};

        iconWrapper.appendChild(saveIcon);
        saveSpan.appendChild(iconWrapper);
        tweetStats.appendChild(saveSpan);
    }});
    ");
          
              _webView.WebMessageReceived += async (s, args) =>
              {
                  var message = args.TryGetWebMessageAsString();
                  await HandleWebMessage(message, data, settings);
              };
      }
    }

    private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!this.IsInitialized || !this.IsLoaded) return;
        try
        {
            SettingsMain settingsMain = new SettingsMain
            {
                MainWindowWidth = e.NewSize.Width,
                MainWindowHeight = e.NewSize.Height
            };
            settingsMain.Save();
        }
        catch (Exception exception)
        {
            var err = MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (err == MessageBoxResult.OK)
            {
                Environment.Exit(1);
            }
        }
    }

    private void Window_OnMouseMoveEvent(object sender, MouseEventArgs e)
    {
      this.DragMove();
    }

    private void OpenSetting_OnClick(object sender, RoutedEventArgs e)
    {
       SettingWindow settingWindow = new SettingWindow(Window);
         settingWindow.Show();
         ViewModel.IsOpen = false;
    }
}