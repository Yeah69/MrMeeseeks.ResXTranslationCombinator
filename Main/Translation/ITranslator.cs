using System.Globalization;
using System.Threading.Tasks;

namespace MrMeeseeks.ResXCombinator.Translation
{
    internal interface ITranslator
    {
        Task<CultureInfo[]> GetSupportedCultureInfos();
        Task<string[]> Translate(
            string[] sourceTexts,
            CultureInfo targetCulture);
    }
}