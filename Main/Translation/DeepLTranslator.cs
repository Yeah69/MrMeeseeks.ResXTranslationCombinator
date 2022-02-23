using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using DeepL;
using MrMeeseeks.ResXTranslationCombinator.Utility;

namespace MrMeeseeks.ResXTranslationCombinator.Translation;

internal interface IDeepLTranslator : ITranslator
{
        
}
    
internal class DeepLTranslator : IDeepLTranslator
{
    private readonly ILogger _logger;
    private readonly Translator _deepLClient;
    private IImmutableSet<CultureInfo>? _cachedSupportedCultureInfos;

    public DeepLTranslator(
        IActionInputs actionInputs,
        Func<string, Translator> deepLClientFactory,
        ILogger logger)
    {
        _logger = logger;
        _deepLClient = deepLClientFactory(actionInputs.AuthKey);
    }

    public bool TranslationsShouldBeCached => true;

    public async Task<IImmutableSet<CultureInfo>> GetSupportedCultureInfos()
    {
        return _cachedSupportedCultureInfos ??= await Inner().ConfigureAwait(false);

        async Task<IImmutableSet<CultureInfo>> Inner()
        {
            try
            {
                _logger.FileLessNotice("Fetching supported CultureInfos from DeepL");   
                var ret = (await _deepLClient.GetTargetLanguagesAsync().ConfigureAwait(false))
                    .Select(sl => sl.CultureInfo)
                    .ToArray();
                _logger.FileLessNotice($"Currently DeepL supports following CultureInfos: {string.Join(", ", ret.Select(ci => ci.Name))}");
                return ImmutableHashSet.Create(ret);
            }
            catch (Exception e)
            {
                _logger.FileLessError($"Fetching supported CultureInfos from DeepL failed: {e}");
                throw;
            }
        }
    }

    private static readonly Regex HotkeyPrefixRegex = new("&([a-zA-Z0-9])", RegexOptions.Compiled);
    private static readonly Regex PlaceholderRegex = new("{([0-9])}", RegexOptions.Compiled);
    private static readonly Regex PlaceholderReverseRegex = new("<placeholder>([0-9])</placeholder>", RegexOptions.Compiled);

    public async Task<string[]> Translate(
        string[] sourceTexts, 
        CultureInfo targetLanguageCode)
    {
        try
        {
            var translations = await _deepLClient.TranslateTextAsync(
                sourceTexts.Select(t => PlaceholderRegex.Replace(
                    HttpUtility.HtmlEncode(HotkeyPrefixRegex.Replace(t, "$1")),
                    "<placeholder>$1</placeholder>")),
                null,
                targetLanguageCode.Name,
                new TextTranslateOptions
                {
                    PreserveFormatting = true,
                    TagHandling = "xml",
                    IgnoreTags = { "placeholder" }
                }
            );

            return translations.Select(t => PlaceholderReverseRegex.Replace(HttpUtility.HtmlDecode(t.Text), "{$1}")).ToArray();
        }
        catch (Exception exception)
        {
            throw new ResXTranslatorTranslationFailedException(exception);
        }
    }
}