using System.Globalization;
using System.Threading.Tasks;

namespace MrMeeseeks.ResXTranslationCombinator.Translation;

internal interface ICopyTranslator : ITranslator
{
        
}
    
internal class CopyTranslator : ICopyTranslator
{
    private readonly IDeepLTranslator _deepLTranslator;

    public CopyTranslator(IDeepLTranslator deepLTranslator) => _deepLTranslator = deepLTranslator;

    public bool TranslationsShouldBeCached => false;
    public Task<IImmutableSet<CultureInfo>> GetSupportedCultureInfos() => _deepLTranslator.GetSupportedCultureInfos();

    public Task<string[]> Translate(
        string[] sourceTexts, 
        CultureInfo targetLanguageCode) => Task.FromResult(sourceTexts);
}