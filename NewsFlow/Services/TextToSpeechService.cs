using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsFlow.Services
{
    public class TextToSpeechService
    {
        public static async Task SpeakAsync(string text, CancellationToken token, string language = "ro")
        {
            var locales = await TextToSpeech.GetLocalesAsync();
            var roLocale = locales?.FirstOrDefault(l => l.Language.StartsWith("ro"));
            await TextToSpeech.SpeakAsync(text, new SpeechOptions
            {
                Locale = roLocale,
                Volume = 1.0f,
                Pitch = 1.0f
            }, token);
        }
    }

}
