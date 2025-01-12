using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using CodeYesterday.Lovi.Session;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Radzen;

namespace CodeYesterday.Lovi
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("********** OMG! FirstChanceException **********");
                System.Diagnostics.Debug.WriteLine(e.Exception);
            };

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddRadzenComponents();

            builder.Services.AddTransient(typeof(IUserSettingsService<>), typeof(MauiUserSettingsService<>));
            builder.Services.AddTransient<IMruService, MruService>();
            builder.Services.AddSingleton<IImporterManager, ImporterManager>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<IProgressIndicator, ProgressIndicator>();
            builder.Services.AddSingleton<AppModel>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
