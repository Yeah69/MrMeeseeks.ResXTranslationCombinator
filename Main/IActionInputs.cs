namespace MrMeeseeks.ResXTranslationCombinator;

public interface IActionInputs
{
    string Directory { get; }
    string AuthKey { get; }
    string SourceLang { get; }
    string ExcludesRegex { get; }
    string DataCopiesRegex { get; }
    bool TakeOverridesKeysSuperSetAsKeyFilter { get; } 
    string LocalizationFilter { get; }
    string LocalizationExcludes { get; }
}