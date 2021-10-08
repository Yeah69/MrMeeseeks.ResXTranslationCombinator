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
            "excludes",
            Required = false,
            Default = "[]",
            HelpText = "Default files or directories to exclude. Relative paths are relative to directory given by 'd'/'dir'.")]
        public string Excludes { get; set; } = "[]";
    }
}
