using System.Threading.Tasks;
using CommandLine;
using MrMeeseeks.ResXTranslationCombinator.Action;
using StrongInject;
using static CommandLine.Parser;

await Default.ParseArguments(() => new ActionInputs(), args)
    .WithNotParsed(_ => Environment.Exit(2))
    .WithParsedAsync(StartAnalysisAsync);

static async Task StartAnalysisAsync(ActionInputs inputs)
{
    await new StrongInjectContainer(inputs)
        .RunAsync(c => c.TraverseAndTranslate())
        .ConfigureAwait(false);
    
    Environment.Exit(0);
}