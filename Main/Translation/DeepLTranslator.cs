using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DeepL;
using MrMeeseeks.ResXTranslationCombinator.Utility;

namespace MrMeeseeks.ResXTranslationCombinator.Translation
{
    internal interface IDeepLTranslator : ITranslator
    {
        
    }
    
    internal class DeepLTranslator : IDeepLTranslator
    {
        private readonly ILogger _logger;
        private readonly DeepLClient _deepLClient;
        private HashSet<CultureInfo>? _cachedSupportedCultureInfos;

        public DeepLTranslator(
            IActionInputs actionInputs,
            Func<string, DeepLClient> deepLClientFactory,
            ILogger logger)
        {
            _logger = logger;
            _deepLClient = deepLClientFactory(actionInputs.AuthKey);
        }

        public bool TranslationsShouldBeCached => true;

        public async Task<HashSet<CultureInfo>> GetSupportedCultureInfos()
        {
            return _cachedSupportedCultureInfos ??= await Inner().ConfigureAwait(false);

            async Task<HashSet<CultureInfo>> Inner()
            {
                try
                {
                    _logger.FileLessNotice("Fetching supported CultureInfos from DeepL");   
                    var ret = (await _deepLClient.GetSupportedLanguagesAsync().ConfigureAwait(false))
                        .Select(sl => CultureInfo.GetCultureInfo(sl.LanguageCode))
                        .ToArray();
                    _logger.FileLessNotice($"Currently DeepL supports following CultureInfos: {string.Join(", ", ret.Select(ci => ci.Name))}");
                    return new HashSet<CultureInfo>(ret);
                }
                catch (Exception e)
                {
                    _logger.FileLessError($"Fetching supported CultureInfos from DeepL failed: {e}");
                    throw;
                }
            }
        }

        public async Task<string[]> Translate(
            string[] sourceTexts, 
            CultureInfo targetLanguageCode)
        {
            try
            {
                var translations = await _deepLClient.TranslateAsync(
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
