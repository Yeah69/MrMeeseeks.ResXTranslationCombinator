using System.Globalization;
using System.Threading.Tasks;
using MrMeeseeks.ResXTranslationCombinator.DI;

namespace MrMeeseeks.ResXTranslationCombinator.Translation;

internal interface ICopyTranslator : ITranslator
{
        
}
    
internal class CopyTranslator : ICopyTranslator, IContainerInstance
{
    private readonly IDeepLTranslator _deepLTranslator;

    public CopyTranslator(IDeepLTranslator deepLTranslator) => _deepLTranslator = deepLTranslator;

    public bool TranslationsShouldBeCached => false;
    public Task<IImmutableSet<CultureInfo>> GetSupportedCultureInfos() => _deepLTranslator.GetSupportedCultureInfos();

    public Task<string[]> Translate(
        string[] sourceTexts, 
        CultureInfo targetLanguageCode) => Task.FromResult(sourceTexts);
}