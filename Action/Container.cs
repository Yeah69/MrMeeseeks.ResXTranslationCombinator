using System.Text.RegularExpressions;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.ResXTranslationCombinator.Translation;

[assembly:AllImplementationsAggregation]

namespace MrMeeseeks.ResXTranslationCombinator.Action;

[ConstructorChoice(typeof(Regex), typeof(string))]

[CreateFunction(typeof(IContext), "Create")]
internal sealed partial class Container
{
    private readonly IActionInputs DIE_Factory_ActionInputs;

    public Container(IActionInputs actionInputs) => 
        DIE_Factory_ActionInputs = actionInputs;
}