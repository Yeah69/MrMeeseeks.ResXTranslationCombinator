using CommandLine;

namespace MrMeeseeks.ResXTranslationCombinator.Action;

public class ActionInputs : IActionInputs
{
    [Option(
        'd', 
        "dir",
        Required = false,
        Default = ".",
        HelpText = "The root directory to start recursive searching from.")]
    public string Directory { get; set; } = ".";

    [Option(
        'a', 
        "auth",
        Required = true,
        HelpText = "Auth key for your DeepL API access.")]
    public string AuthKey { get; set; } = "";
        
    [Option(
        's', 
        "source-lang",
        Required = false,
        Default = "",
        HelpText = "The language used in the original ResX files. Leave empty to auto-detect.")]
    public string SourceLang { get; set; } = "";

    [Option(
        'g',
        "glossary-name",
        Required = false,
        Default = "",
        HelpText = "The name of the glossary to use for translation. Only works if source-lang is also set.")]
    public string GlossaryName { get; set; } = "";
        
    [Option(
        'e', 
        "excludes-regex",
        Required = false,
        Default = "",
        HelpText = "Regex for names of default ResX files in order to decide whether to exclude file from processing.")]
    public string ExcludesRegex { get; set; } = "";
        
    [Option(
        'c', 
        "data-copies-regex",
        Required = false,
        Default = "",
        HelpText = "Regex for names of default ResX files whose data should be copied instead of translated.")]
    public string DataCopiesRegex { get; set; } = "";
        
    [Option(
        'f', 
        "filter-keys-with-overrides",
        Required = false,
        Default = false,
        HelpText = "If set the default keys are filtered by the super set of override keys per ResX file family.")]
    public bool TakeOverridesKeysSuperSetAsKeyFilter { get; set; }

    [Option(
        'l',
        "localizations-filter",
        Required = false,
        Default = "",
        HelpText = "Concat CultureInfo-/language-codes joined with ';' in order to filter supported languages from the translation provider. Not usable in combination with localizations-excludes.")]
    public string LocalizationFilter { get; set; } = "";

    [Option(
        'x',
        "localizations-excludes",
        Required = false,
        Default = "",
        HelpText = "Concat CultureInfo-/language-codes joined with ';' in order to exclude supported languages from the translation provider. Not usable in combination with localizations-filter.")]
    public string LocalizationExcludes { get; set; } = "";

    public override string ToString() => 
        $"{nameof(Directory)}: {Directory}, {nameof(AuthKey)}: {AuthKey}, {nameof(SourceLang)}: {SourceLang}, {nameof(GlossaryName)}: {GlossaryName}, {nameof(ExcludesRegex)}: {ExcludesRegex}, {nameof(DataCopiesRegex)}: {DataCopiesRegex}, {nameof(TakeOverridesKeysSuperSetAsKeyFilter)}: {TakeOverridesKeysSuperSetAsKeyFilter}, {nameof(LocalizationFilter)}: {LocalizationFilter}, {nameof(LocalizationExcludes)}: {LocalizationExcludes}";
}