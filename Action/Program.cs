using System.Threading.Tasks;
using CommandLine;
using MrMeeseeks.ResXTranslationCombinator.Action;

using static CommandLine.Parser;

await Default.ParseArguments(() => new ActionInputs(), args)
    .WithNotParsed(_ => Environment.Exit(2))
    .WithParsedAsync(StartAnalysisAsync);

static async Task StartAnalysisAsync(ActionInputs inputs)
{
    await using var container = new Container(inputs);
    await container.Create().TraverseAndTranslate().ConfigureAwait(false);
    Environment.Exit(0);
}