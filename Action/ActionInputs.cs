using System;
using CommandLine;

namespace MrMeeseeks.ResXTranslationCombinator.Action
{
    public class ActionInputs
    {
        public ActionInputs()
        {
            var greetings = Environment.GetEnvironmentVariable("GREETINGS");
            if (greetings is { Length: > 0 })
            {
                Console.WriteLine(greetings);
            }
        }

        [Option('d', "dir",
            Required = true,
            HelpText = "The root directory to start recursive searching from.")]
        public string Directory { get; set; } = null!;

        [Option('a', "auth",
            Required = true,
            HelpText = "Auth key for your DeepL API access.")]
        public string AuthKey { get; set; } = null!;
    }
}
