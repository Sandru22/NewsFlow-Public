using Android.Content;
using NewsFlow;


[assembly: Dependency(typeof(TextToSpeechSettingsOpener))]
namespace NewsFlow
{
    public class TextToSpeechSettingsOpener : ITextToSpeechSettingsOpener
    {
        public async Task OpenTtsSettings()
        {
            try
            {
                var context = Android.App.Application.Context;
                if (context == null) return;

                var intents = new List<Intent>
                {
                    new Intent("com.android.settings.TTS_SETTINGS"),
                    new Intent(Android.Provider.Settings.ActionAccessibilitySettings),
                    new Intent(Intent.ActionView,
                        Android.Net.Uri.Parse("market://details?id=com.google.android.tts"))
                };

                foreach (var intent in intents)
                {
                    try
                    {
                        intent.AddFlags(ActivityFlags.NewTask);
                        context.StartActivity(intent);
                        await Task.Delay(500);
                        return;
                    }
                    catch { }
                }

                // Fallback ultimă variantă
                var webIntent = new Intent(
                    Intent.ActionView,
                    Android.Net.Uri.Parse("https://play.google.com/store/apps/details?id=com.google.android.tts"));
                webIntent.AddFlags(ActivityFlags.NewTask);
                context.StartActivity(webIntent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la deschiderea setărilor: {ex.Message}");
            }
        }
    }
}