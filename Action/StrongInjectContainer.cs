using MrMeeseeks.ResXTranslationCombinator.ResX;
using MrMeeseeks.ResXTranslationCombinator.Translation;
using StrongInject;
using StrongInject.Modules;

namespace MrMeeseeks.ResXTranslationCombinator.Action
{
    [Register(typeof(DeepLContext), typeof(IContext))]
    [Register(typeof(ResXTranslator), typeof(IResXTranslator))]
    [Register(typeof(DeepLTranslator), typeof(ITranslator))]
    
    [Register(typeof(ResXNode), typeof(IResXNode))]
    [Register(typeof(ResXReader), typeof(IResXReader))]
    [Register(typeof(ResXWriter), typeof(IResXWriter))]
    [Register(typeof(ResXWriterFactory), typeof(IResXWriterFactory))]
    [Register(typeof(ResXElementsFactory), typeof(IResXElementsFactory))]
    
    [RegisterModule(typeof(StandardModule))]
    internal partial class StrongInjectContainer : IAsyncContainer<IContext>
    {
        [Instance] private IActionInputs ActionInputs { get; }

        public StrongInjectContainer(
            IActionInputs actionInputs) =>
            ActionInputs = actionInputs;
    }
}