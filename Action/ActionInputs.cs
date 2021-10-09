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

        public override string ToString() => 
            $"{nameof(ActionInputs)} => {nameof(Directory)}: {Directory}, {nameof(AuthKey)}: {AuthKey}, {nameof(ExcludesRegex)}: {ExcludesRegex}, {nameof(DataCopiesRegex)}: {DataCopiesRegex}";
    }
}
