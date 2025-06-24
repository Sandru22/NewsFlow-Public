using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Plugin.LocalNotification;
using Plugin.Firebase.CloudMessaging;
using Plugin.FirebasePushNotification;
using NewsFlow.Web;
using NewsFlow.Models;
#if ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif


namespace NewsFlow
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification()
                .RegisterFirebaseServices()

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif



            return builder.Build();
        }

        private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
        {
            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity)));
#endif
            });

            return builder;
        }

        
    }
}
