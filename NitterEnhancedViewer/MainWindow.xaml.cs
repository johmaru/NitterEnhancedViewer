using System.ComponentModel;
using System.Security.Policy;
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
    private bool _devMode;
    
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

    private static bool IsValidJson(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return false;
        }
        
        try 
        {
            JsonDocument.Parse(str);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            SettingsMain settingsMain = new SettingsMain();
            this.Width = settingsMain.MainWindowWidth;
            this.Height = settingsMain.MainWindowHeight;

            var json = await JsonController.LoadSettings();
            _devMode = json.DevMode;
            
            _webView = FindVisualChild<WebView2>(Window);
            if (_webView == null) return;
            
            await _webView.EnsureCoreWebView2Async();
            
            _webView.Source = new Uri(_nitterUrl);
            _webView.NavigationCompleted += WebViewOnNavigationCompleted;
            _webView.SourceChanged += WebViewOnSourceChanged;
            _webView.CoreWebView2.ContextMenuRequested += CoreWebView2OnContextMenuRequested;
            
            FavoritesWindow.OnClicked.Subscribe(url =>
            {
                var absoluteUrl = "https://nitter.net/" + url;
                _webView.Source = new Uri(absoluteUrl);
                this.Activate();
            });
           
            
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

    private async void CoreWebView2OnContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs e)
    {
        try
        {
            e.MenuItems.Clear();

            // Add a custom menu item
            var settingsMenuItem = _webView.CoreWebView2.Environment.CreateContextMenuItem(
                LanguageController.GetLocalizedValue<string>("S_Settings"),
                null,
                CoreWebView2ContextMenuItemKind.Command
            );
       
            //Add a click event to the custom menu item
            settingsMenuItem.CustomItemSelected += (o, args) =>
            {
                SettingWindow settingWindow = new SettingWindow(Window);
                settingWindow.Show();
            };
       
            // Add Items to the context menu
            e.MenuItems.Add(settingsMenuItem);
         
            /*
          *  DEV MODE
          */
            
            if ((_devMode))
            {
                var devMenuItem = _webView.CoreWebView2.Environment.CreateContextMenuItem(
                    "DevTools",
                    null,
                    CoreWebView2ContextMenuItemKind.Command
                );
                devMenuItem.CustomItemSelected += (o, args) =>
                {
                    _webView.CoreWebView2.OpenDevToolsWindow();
                };
                e.MenuItems.Add(devMenuItem);
            }
        }
        catch (Exception ex)
        {
            var err = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (err == MessageBoxResult.OK)
            {
                Environment.Exit(1);
            }
        }
    }

    private async void WebViewOnSourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
        if (_webView == null || !_webView.Source.AbsoluteUri.Contains("nitter.net"))
        {
            _webView.Source = new Uri(_nitterUrl);
        }
        else
        {
            // Add custom navigation items to the nitter.net page
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

                const FavoriteItem = document.createElement('div');
                FavoriteItem.className = 'nav-item';
                const FavoriteLink = document.createElement('a');
                FavoriteLink.href = '#';
                FavoriteLink.textContent = '{LanguageController.GetLocalizedValue<string>("S_FavoriteList")}';
                FavoriteLink.onclick = (e) => {{
                    e.preventDefault();
                    window.chrome.webview.postMessage('favorites');
                }};

                const TimelineItem = document.createElement('div');
                TimelineItem.className = 'nav-item';
                const TimelineLink = document.createElement('a');
                TimelineLink.href = '#';
                TimelineLink.textContent = '{LanguageController.GetLocalizedValue<string>("S_Timeline")}';
                TimelineLink.onclick = (e) => {{
                    e.preventDefault();
                    window.chrome.webview.postMessage('timeline');
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

                navItem.appendChild(newNavItem);
                newNavItem.appendChild(newLink);

                navItem.appendChild(FavoriteItem);
                FavoriteItem.appendChild(FavoriteLink);

                navItem.appendChild(TimelineItem);
                TimelineItem.appendChild(TimelineLink);

                navItem.appendChild(TweetItem);
                TweetItem.appendChild(TweetLink);
            }}
        ");
        }
        
    }
    
    private async Task HandleWebMessage(string? message, List<NitterData> data, SettingJson  settings)
    {
        if (message == null) return;

        if (IsValidJson(message))
        {
            try
            {
                var parsedMessage = JsonSerializer.Deserialize<Dictionary<string, String>>(message);
                if (parsedMessage == null) return;
                var tweetHref = parsedMessage["tweetHref"];
                var tweetMessage = parsedMessage["message"];
                var existingData = data.FirstOrDefault(x => x.Url == tweetHref);
                
                if (existingData != null)
                {
                    await JsonController.RemoveNitterData(existingData);
                    data.Remove(existingData);
                }
                else
                {
                    var newData = new NitterData
                    {
                        Message = tweetMessage,
                        Url = tweetHref,
                        FavoriteTime = DateTime.Now,
                        IsFavorite = true
                    };
                    await JsonController.AppendNitterData(newData);
                    data.Add(newData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        switch (message)
        {
            case "profile":
                if (string.IsNullOrEmpty(settings.Xusername))
                {
                    var msg = MessageBox.Show(LanguageController.GetLocalizedValue<string>("M_DidntXaccountSettingAndAskOpen"),
                        "Warning", MessageBoxButton.YesNo);
                    if (msg == MessageBoxResult.Yes)
                    {
                        if (Application.Current.Windows.OfType<SettingWindow>().Any())
                        {
                            Application.Current.Windows.OfType<SettingWindow>().First().Activate();
                        }
                        else
                        {
                            SettingWindow settingWindow = new SettingWindow(Window);
                            settingWindow.Show();
                            settingWindow.Activate();
                        }
                    }
                }
                _webView.Source = new Uri($"https://nitter.net/{settings.Xusername}");
                return;

            case "timeline":
            {
                if (Application.Current.Windows.OfType<TimelineWindow>().Any())
                {
                    Application.Current.Windows.OfType<TimelineWindow>().First().Activate();
                }
                else
                {
                    TimelineWindow timelineWindow = new TimelineWindow();
                    timelineWindow.Show();
                    timelineWindow.Activate();
                }
                return;
            }
            case "tweet":
            {
               if (Application.Current.Windows.OfType<TweetWindow>().Any())
               {
                   Application.Current.Windows.OfType<TweetWindow>().First().Activate();
               }
               else
               {
                   TweetWindow tweetWindow = new TweetWindow();
                   tweetWindow.Show();
                   tweetWindow.Activate();
               }
               return;
            }
            case "favorites":
            {
                if (Application.Current.Windows.OfType<FavoritesWindow>().Any())
                {
                    Application.Current.Windows.OfType<FavoritesWindow>().First().Activate();
                }
                else
                {
                    FavoritesWindow favoritesWindow = new FavoritesWindow();
                    favoritesWindow.Show();
                    favoritesWindow.Activate();
                }
                return;
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
          
          
          // Inject custom css and js
          await _webView.ExecuteScriptAsync($@"
    window.updateFavoriteUrls = function(newUrls) {{
        window.favoriteUrls = newUrls;
      
        document.querySelectorAll('.custom-heart').forEach(icon => {{
            const tweetHref = icon.closest('.timeline-item').querySelector('.tweet-link').getAttribute('href');
            const normalizedHref = tweetHref.startsWith('/') ? tweetHref.substring(1) : tweetHref;
            const isSaved = newUrls.includes(normalizedHref);
            icon.style.color = isSaved ? '#e0245e' : '#00BA7C';
            icon.title = isSaved ? 
                '{LanguageController.GetLocalizedValue<string>("S_SavedIconTrue")}' : 
                '{LanguageController.GetLocalizedValue<string>("S_SavedIconFalse")}';
        }});
    }};
");
          
          // Add save button to each tweet
         await _webView.ExecuteScriptAsync($@"
        const timelineItems = document.querySelectorAll('.timeline-item');
        const favoriteUrls = {JsonSerializer.Serialize(data.Select(x => x.Url))};

        const style = document.createElement('style');
        style.textContent = `
        @font-face {{
        font-family: 'fontello';
        src: url('/fonts/fontello.eot?21002321');
        src: url('/fonts/fontello.eot?21002321#iefix') format('embedded-opentype'),
             url('/fonts/fontello.woff2?21002321') format('woff2'),
             url('/fonts/fontello.woff?21002321') format('woff'),
             url('/fonts/fontello.ttf?21002321') format('truetype'),
             url('/fonts/fontello.svg?21002321#fontello') format('svg');
        font-weight: normal;
        font-style: normal;
    }}

    [class^='icon-']:before, [class*=' icon-']:before {{
        font-family: 'fontello';
        font-style: normal;
        font-weight: normal;
        speak: never;
        display: inline-block;
        text-decoration: inherit;
        width: 1em;
        text-align: center;
        font-variant: normal;
        text-transform: none;
        line-height: 1em;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
    }}

    .icon-heart:before {{ content: '\\2665'; }}

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
        .custom-heart {{
    font-family: 'fontello';
    font-style: normal;
    font-weight: normal;
    speak: never;
    display: inline-block;
    text-decoration: inherit;
    width: 1em;
    text-align: center;
    font-variant: normal;
    text-transform: none;
    line-height: 1em;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    color: inherit;
}}
 .custom-heart:before {{
        content: '\\2665';
    }}
      `;
        document.head.appendChild(style);

        timelineItems.forEach(item => {{
            const tweetLink = item.querySelector('a.tweet-link');
            if (!tweetLink) return;

            const tweetBody = item.querySelector('.tweet-body');
            if (!tweetBody) return;

            const tweetContent = tweetBody.querySelector('.tweet-content.media-body');

            if (!tweetContent) return;

            const rawHref = tweetLink.getAttribute('href');
            const tweetHref = rawHref.startsWith('/') ? rawHref.substring(1) : rawHref;
            const tweetStats = item.querySelector('.tweet-stats');
            if (!tweetStats) return;

     
            if (tweetStats.querySelector('.save-button')) return;

            
            const saveSpan = document.createElement('span');
            saveSpan.className = 'tweet-stat save-button';

            const iconWrapper = document.createElement('div');
            iconWrapper.className = 'icon-container';

            const saveIcon = document.createElement('span');
            const isSaved = favoriteUrls.includes(tweetHref);
            saveIcon.className = 'custom-heart';
            saveIcon.title = isSaved ? '{LanguageController.GetLocalizedValue<string>("S_SavedIconTrue")}' : '{LanguageController.GetLocalizedValue<string>("S_SavedIconFalse")}';;
            saveIcon.style.color = isSaved ? '#e0245e' : '#00BA7C';

            iconWrapper.onclick = (e) => {{
                e.preventDefault();
                e.stopPropagation();
                window.chrome.webview.postMessage(JSON.stringify({{
                    action: 'save',
                    tweetHref: tweetHref,
                    message: tweetContent.innerText
                }}));
                const currentColor = saveIcon.style.color;
                const isCurrentlySaved = currentColor === '#e0245e';
                saveIcon.style.color = isCurrentlySaved ? '#00BA7C' : '#e0245e';
                saveIcon.title = isCurrentlySaved ? 
            '{LanguageController.GetLocalizedValue<string>("S_SavedIconFalse")}' : 
            '{LanguageController.GetLocalizedValue<string>("S_SavedIconTrue")}';
            }};

        iconWrapper.appendChild(saveIcon);
        saveSpan.appendChild(iconWrapper);
        tweetStats.appendChild(saveSpan);
    }});
    ");
          
              _webView.WebMessageReceived += async (s, args) =>
              {
                  var message = args.TryGetWebMessageAsString();
                    if (message == null) return;
                  await HandleWebMessage(message, data, settings);
                  var updatedUrls = JsonSerializer.Serialize(data.Select(x => x.Url));
                  await _webView.ExecuteScriptAsync($@"window.updateFavoriteUrls({updatedUrls});");
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

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
       if(_webView != null) _webView.Dispose();
    }
}