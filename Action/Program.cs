using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using MrMeeseeks.ResXTranslationCombinator.Action;
using MrMeeseeks.ResXTranslationCombinator.Translation;
using static CommandLine.Parser;

static async Task StartAnalysisAsync(ActionInputs inputs)
{
    var defaultResXFiles = Directory.EnumerateFiles(inputs.Directory, "*.resx", SearchOption.AllDirectories)
        .Select(p => new FileInfo(p))
        .Where(fi => fi.Name.Split('.').Length == 2)
        .ToList();

    var resXTranslator = ContextFactory.CreateDeepLContext(inputs.AuthKey).Translator;
    
    foreach (var defaultResXFile in defaultResXFiles)
    {
        await resXTranslator.Translate(defaultResXFile.FullName).ConfigureAwait(false);
    }

    var title = "Localization";
    var summary = "Summary";

    // https://docs.github.com/actions/reference/workflow-commands-for-github-actions#setting-an-output-parameter
    Console.WriteLine($"::set-output name=summary-title::{title}");
    Console.WriteLine($"::set-output name=summary-details::{summary}");

    Environment.Exit(0);
}

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(_ => Environment.Exit(2));

await parser.WithParsedAsync(StartAnalysisAsync);
