using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NitterEnhancedViewer.lib.localization;

namespace NitterEnhancedViewer;

public partial class LoginXWindow : Window
{
    private WebView2? _webView;
    
    public LoginXWindow()
    {
        InitializeComponent();
    }

    private void Window_OnMouseMoveEvent(object sender, MouseEventArgs e)
    {
      this.DragMove();
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

    private void LoginXWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        _webView = FindVisualChild<WebView2>(Window);
        if (_webView == null) return;
        _webView.Source = new Uri("https://x.com/i/flow/login");
        _webView.NavigationCompleted += WebViewOnNavigationCompleted;
        _webView.SourceChanged += WebViewOnSourceChanged;
    }

    private void WebViewOnSourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
        if (_webView != null && _webView.Source.AbsoluteUri == "https://x.com/i/flow/login")
        {
            var box = MessageBox.Show(LanguageController.GetLocalizedValue<string>("ST_WarningUseXAccount"), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (box == MessageBoxResult.No)
            {
                Environment.Exit(1);
            }
        }
        
        if (_webView!= null && _webView.Source.AbsoluteUri == "https://x.com/home")
        {
            Close();
        }
    }

    private void WebViewOnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess) return;
    }
}