using Microsoft.Extensions.Logging;
using MobileApp.Services;
using MobileApp.ViewModels;
using MobileApp.Pages;

namespace MobileApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register HttpClient
            builder.Services.AddSingleton(sp =>
                new HttpClient { BaseAddress = new Uri("https://localhost:7138/api/bot/") });
            
            // Register Services
            builder.Services.AddSingleton<IBotControlService, BotControlService>();
            builder.Services.AddSingleton<ICacheService, CacheService>();

            // Register Pages
            builder.Services.AddSingleton<DashboardPage>();
            builder.Services.AddSingleton<HistoryPage>();
            builder.Services.AddSingleton<AnalyticsPage>();
            builder.Services.AddSingleton<EventsPage>();

            // Register ViewModels
            builder.Services.AddSingleton<DashboardPageViewModel>();
            builder.Services.AddSingleton<HistoryPageViewModel>();
            builder.Services.AddSingleton<AnalyticsPageViewModel>();
            builder.Services.AddSingleton<EventsPageViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

