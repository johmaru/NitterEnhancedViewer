using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NitterEnhancedViewer.lib.localization;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace NitterEnhancedViewer;

public partial class TweetWindow : FluentWindow
{
    private static readonly string _url = "https://x.com/compose/post";
    private WebView2? _webView;
    
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
    
    public TweetWindow()
    {
        InitializeComponent();
       
    }
    
    private WebView2? FindWebView()
    {
        return this.FindName("WebView") as WebView2 
               ?? FindVisualChild<WebView2>(this.Content as DependencyObject);
    }

    private void Window_OnMouseMoveEvent(object sender, MouseEventArgs e)
    {
        this.DragMove();
    }

    private async void TweetWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        _webView = FindWebView();
        if (_webView == null)
        {
            return;
        }

        _webView.CoreWebView2InitializationCompleted += WebViewOnCoreWebView2InitializationCompleted;
        await _webView.EnsureCoreWebView2Async();
        _webView.Source = new Uri(_url);
      
    }

    private void WebViewOnCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (!e.IsSuccess || _webView?.CoreWebView2 == null) return;
        
        var settings = _webView.CoreWebView2.Settings;
        settings.IsWebMessageEnabled = true;
        settings.IsStatusBarEnabled = true;
        settings.AreDefaultScriptDialogsEnabled = true;
        settings.IsScriptEnabled = true;
        
        _webView.SourceChanged += WebViewOnSourceChanged;
        _webView.CoreWebView2.NavigationStarting += CoreWebView2OnNavigationStarting;
        _webView.CoreWebView2.WebResourceResponseReceived += CoreWebView2OnWebResourceResponseReceived;
        _webView.CoreWebView2.NewWindowRequested += CoreWebView2OnNewWindowRequested;
        _webView.CoreWebView2.FrameNavigationStarting += CoreWebView2OnFrameNavigationStarting;
        _webView.CoreWebView2.WebMessageReceived += CoreWebView2OnWebMessageReceived;

        _webView.CoreWebView2.DOMContentLoaded += async (s, e) =>
        { 
            var script = @"
        setTimeout(() => {
            try {
                const links = document.head.getElementsByTagName('link');
                for (let i = 0; i < links.length; i++) {
                    if (links[i].rel === 'canonical' && links[i].href.includes('flow/login')) {
                        console.log('Found target link');
                        chrome.webview.postMessage('LOGIN_NOT_DETECTED');
                        return;
                    }
                }
            } catch (err) {
                console.error('Error:', err);
            }
        }, 1000);
         ";

            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        };
    }

    private void CoreWebView2OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        if  (e.WebMessageAsJson.Contains("LOGIN_NOT_DETECTED"))
        {
            Dispatcher.Invoke(() =>
            {
                var box = MessageBox.Show(LanguageController.GetLocalizedValue<string>("M_DidntXaccountSetting"),
                    "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
                if (box == MessageBoxResult.OK)
                {
                    Close();
                }
            });
        }
    }

    private void CoreWebView2OnFrameNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        CheckForLoginFlow(e.Uri);
    }

    private void CoreWebView2OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
       CheckForLoginFlow(e.Uri);
    }

    private void CoreWebView2OnWebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        var statusCode = e.Response.StatusCode;
        var headers = e.Response.Headers;
        var location = headers.GetHeader("Location");
        
        if (location != null)
        {
            if (location.Contains("/flow/login"))
            {
                Dispatcher.Invoke(() =>
                {
                    var box = MessageBox.Show(LanguageController.GetLocalizedValue<string>("ST_WarningUseXAccount"),
                        "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (box == MessageBoxResult.No)
                    {
                        Environment.Exit(1);
                    }
                });
            }
        }
    }

    private void CoreWebView2OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        CheckForLoginFlow(e.Uri);
    }
    
    private void CheckForLoginFlow(string uri)
    {
        if (uri.Contains("/flow/login"))
        {
            Dispatcher.Invoke(() =>
            {
                var box = MessageBox.Show(LanguageController.GetLocalizedValue<string>("ST_WarningUseXAccount"),
                    "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (box == MessageBoxResult.No)
                {
                    Environment.Exit(1);
                }
            });
        }
    }

    private void WebViewOnSourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
        
        if (_webView != null && _webView.Source.AbsoluteUri == "https://x.com/home")
        {
            Close();
        }
    }

    private void TweetWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (_webView == null) return;
        _webView.Source = new Uri("about:blank");
        _webView.Dispose();
    }
}