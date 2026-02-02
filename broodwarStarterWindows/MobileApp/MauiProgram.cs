using Microsoft.Extensions.Logging;
using MobileApp.Services;
using MobileApp.ViewModels;

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
            builder.Services.AddSingleton<IBotControlService, BotControlService>();
            builder.Services.AddSingleton<HomePageViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
