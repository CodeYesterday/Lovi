﻿using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace CodeYesterday.Lovi;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // Avoid white background flicker on startup with dark theme:
        if (Preferences.Default.Get($"{nameof(SettingsService)}.Theme", "dark")
            .EndsWith("dark", StringComparison.OrdinalIgnoreCase))
        {
            BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping("MyBlazorCustomization", (handler, view) =>
            {
#if IOS
                handler.PlatformView.Opaque = false;
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#elif WINDOWS
                handler.PlatformView.Opacity = 1;
                handler.PlatformView.DefaultBackgroundColor = new Windows.UI.Color { A = 0, R = 0, G = 0, B = 0 };
#endif
            });
        }

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
