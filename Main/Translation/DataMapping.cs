using System.Collections.Immutable;
using System.Globalization;

namespace MrMeeseeks.ResXTranslationCombinator.Translation
{
    public interface IDataMapping
    {
        IImmutableDictionary<string, string> Default { get; } 
        IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Automatics { get; set; }
        IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Overrides { get; }
    }

    internal record DataMapping : IDataMapping
    {
        public DataMapping((IImmutableDictionary<string, string> Default,
            IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Automatics,
            IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Overrides) tuple) => (Default, Automatics, Overrides) = tuple;

        public IImmutableDictionary<string, string> Default { get; } 
        public IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Automatics { get; set; }
        public IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Overrides { get; }
    };
}