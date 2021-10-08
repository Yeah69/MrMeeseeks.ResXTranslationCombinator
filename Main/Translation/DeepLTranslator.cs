using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DeepL;

namespace MrMeeseeks.ResXTranslationCombinator.Translation
{
    internal class DeepLTranslator : ITranslator
    {
        private readonly string _authKey;

        public DeepLTranslator(
            IActionInputs actionInputs) =>
            _authKey = actionInputs.AuthKey;

        public async Task<CultureInfo[]> GetSupportedCultureInfos()
        {
            using var client = new DeepLClient(_authKey);
            var supportedLanguages = (await client.GetSupportedLanguagesAsync().ConfigureAwait(false)).ToArray();
            return supportedLanguages
                .Select(sl => CultureInfo.GetCultureInfo(sl.LanguageCode))
                .ToArray();
        }

        public async Task<string[]> Translate(
            string[] sourceTexts, 
            CultureInfo targetLanguageCode)
        {
            using var client = new DeepLClient(_authKey);
            try
            {
                var translations = await client.TranslateAsync(
                    sourceTexts,
                    targetLanguageCode.TwoLetterISOLanguageName
                );

                return translations.Select(t => t.Text).ToArray();
            }
            catch (Exception exception)
            {
                throw new ResXTranslatorTranslationFailedException(exception);
            }
        }
    }
}
