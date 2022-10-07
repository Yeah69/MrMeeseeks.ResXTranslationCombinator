using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using DeepL;
using DeepL.Model;
using MrMeeseeks.ResXTranslationCombinator.DI;
using MrMeeseeks.ResXTranslationCombinator.Utility;

namespace MrMeeseeks.ResXTranslationCombinator.Translation;

internal interface IDeepLTranslator : ITranslator
{
        
}
    
internal class DeepLTranslator : IDeepLTranslator, IContainerInstance
{
    private readonly ILogger _logger;
    private readonly Translator _deepLClient;
    private IImmutableSet<CultureInfo>? _cachedSupportedCultureInfos;
    private string? _sourceLanguage;
    private string? _glossaryName;

    public DeepLTranslator(
        IActionInputs actionInputs,
        Func<string, Translator> deepLClientFactory,
        ILogger logger)
    {
        _logger = logger;
        _deepLClient = deepLClientFactory(actionInputs.AuthKey);
        _sourceLanguage = string.IsNullOrEmpty(actionInputs.SourceLang) ? null : actionInputs.SourceLang;
        _glossaryName = string.IsNullOrEmpty(actionInputs.GlossaryName) ? null : actionInputs.GlossaryName;
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
            GlossaryInfo? glossary = null;
            if (_glossaryName != null && _sourceLanguage != null)
            {
                var glossaries = await _deepLClient.ListGlossariesAsync();
                glossary = glossaries.FirstOrDefault(x =>
                    x.Ready &&
                    x.Name == _glossaryName &&
                    x.SourceLanguageCode == _sourceLanguage &&
                    x.TargetLanguageCode == targetLanguageCode.Name);
            }

            var translations = await _deepLClient.TranslateTextAsync(
                sourceTexts.Select(t => PlaceholderRegex.Replace(
                    HttpUtility.HtmlEncode(HotkeyPrefixRegex.Replace(t, "$1")),
                    "<placeholder>$1</placeholder>")),
                _sourceLanguage,
                targetLanguageCode.Name,
                new TextTranslateOptions
                {
                    PreserveFormatting = true,
                    TagHandling = "xml",
                    IgnoreTags = { "placeholder" },
                    GlossaryId = glossary?.GlossaryId
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