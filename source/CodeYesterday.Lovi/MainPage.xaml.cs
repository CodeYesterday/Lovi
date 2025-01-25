namespace CodeYesterday.Lovi;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
#if WINDOWS && !DEBUG
            Loaded += MainPage_Loaded;
#endif
    }

#if WINDOWS && !DEBUG
    private async void MainPage_Loaded(object? sender, EventArgs e)
    {
        var webView2 = (blazorWebView.Handler?.PlatformView as Microsoft.UI.Xaml.Controls.WebView2);
        if (webView2 is null) return;
        await webView2.EnsureCoreWebView2Async();

        var settings = webView2.CoreWebView2.Settings;
        settings.IsZoomControlEnabled = false;
        settings.AreBrowserAcceleratorKeysEnabled = false;
        settings.AreDevToolsEnabled = true;
    }
#endif
}
