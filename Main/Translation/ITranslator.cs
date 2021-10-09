using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace MrMeeseeks.ResXTranslationCombinator.Translation
{
    internal interface ITranslator
    {
        bool TranslationsShouldBeCached { get; }
        Task<HashSet<CultureInfo>> GetSupportedCultureInfos();
        Task<string[]> Translate(
            string[] sourceTexts,
            CultureInfo targetCulture);
    }
}