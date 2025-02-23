using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace NitterEnhancedViewer;

public partial class TimelineWindow : FluentWindow
{
    private static readonly string _timelineUrl = "https://x.com/home";
    private static WebView2? _webView;
    
    public TimelineWindow()
    {
        InitializeComponent();
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

    private void Window_OnMouseMoveEvent(object sender, MouseEventArgs e)
    {
       this.DragMove();
    }

    private async void TimelineWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _webView = FindVisualChild<WebView2>(Window);
            if (_webView == null) return;
            
            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36";
            
            _webView.Source = new Uri(_timelineUrl);
            _webView.SourceChanged += WebViewOnSourceChanged;
            _webView.NavigationCompleted += WebViewOnNavigationCompleted;
        }
        catch (Exception ex)
        {
          var err = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);    
          if (err == MessageBoxResult.OK) Close();
        }
    }

    private async void WebViewOnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        await Task.Delay(1000);
    
        // DOMの変更を監視するスクリプトを追加
        await _webView.ExecuteScriptAsync(@"
        function removeUnwantedLinks() {
            const nav = document.querySelector('nav');
            if (nav) {
                const links = nav.querySelectorAll('a');
                links.forEach(link => {
                    if (link.getAttribute('href') !== '/home') {
                        link.remove();
                    }
                });
            }
        }

        function removeAside() {
            const aside = document.querySelector('aside');
            if (aside) {
                aside.remove();
            }
        }

        // MutationObserverを使用してDOM変更を監視
        const observer = new MutationObserver(() => {
            removeUnwantedLinks();
            removeAside();
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });

        // 初回実行
        removeUnwantedLinks();
        removeAside();
    ");
    }
    

    private void WebViewOnSourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
        if (_webView == null || _webView?.Source == new Uri(_timelineUrl)) return;
        var err = MessageBox.Show("Failed to load the timeline", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        if (err == MessageBoxResult.OK) Close();
        
    }
}