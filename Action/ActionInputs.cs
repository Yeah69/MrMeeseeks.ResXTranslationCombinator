using CommandLine;

namespace MrMeeseeks.ResXTranslationCombinator.Action
{
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

        public override string ToString() => 
            $"{nameof(Directory)}: {Directory}, {nameof(ExcludesRegex)}: {ExcludesRegex}, {nameof(DataCopiesRegex)}: {DataCopiesRegex}, {nameof(TakeOverridesKeysSuperSetAsKeyFilter)}: {TakeOverridesKeysSuperSetAsKeyFilter}";
    }
}
