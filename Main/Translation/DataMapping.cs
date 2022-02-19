using System.Globalization;

namespace MrMeeseeks.ResXTranslationCombinator.Translation;

public interface IDataMapping
{
    IReadOnlySet<string> Keys { get; }
    IImmutableDictionary<string, string> Default { get; } 
    IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Automatics { get; set; }
    IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Overrides { get; }
}

internal record DataMapping : IDataMapping
{
    public DataMapping(
        IReadOnlySet<string> keys,
        (IImmutableDictionary<string, string> Default,
            IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Automatics,
            IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Overrides) tuple)
    {
        Keys = keys;
        (Default, Automatics, Overrides) = tuple;
    }
        
    public IReadOnlySet<string> Keys { get; }
    public IImmutableDictionary<string, string> Default { get; } 
    public IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Automatics { get; set; }
    public IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Overrides { get; }
};