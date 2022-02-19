using System.IO;
using System.Text.RegularExpressions;
using DeepL;
using MrMeeseeks.ResXTranslationCombinator.ResX;
using MrMeeseeks.ResXTranslationCombinator.Translation;
using MrMeeseeks.ResXTranslationCombinator.Utility;
using StrongInject;
using StrongInject.Modules;

namespace MrMeeseeks.ResXTranslationCombinator.Action;

[Register(typeof(DeepLContext), typeof(IContext))]
[Register(typeof(ResXCombinator<>), typeof(IResXCombinator<>))]
[Register(typeof(CopyTranslator), Scope.SingleInstance, typeof(ICopyTranslator))]
[Register(typeof(DeepLTranslator), Scope.SingleInstance, typeof(IDeepLTranslator))]
[Register(typeof(DataMappingFactory), typeof(IDataMappingFactory))]
[Register(typeof(DataMapping), typeof(IDataMapping))]
    
[Register(typeof(ResXNode), typeof(IResXNode))]
[Register(typeof(ResXReader), typeof(IResXReader))]
[Register(typeof(ResXWriter), typeof(IResXWriter))]
[Register(typeof(ResXWriterFactory), typeof(IResXWriterFactory))]
[Register(typeof(ResXElementsFactory), typeof(IResXElementsFactory))]
    
[Register(typeof(Logger), typeof(ILogger))]
    
[Register(typeof(Translator))]
[Register(typeof(FileInfo))]
[Register(typeof(DirectoryInfo))]

[Register(typeof(IntersectingCultureInfosFilter), typeof(IIntersectingCultureInfosFilter))]
[Register(typeof(ExcludingCultureInfosFilter), typeof(IExcludingCultureInfosFilter))]
[Register(typeof(NonChangingCultureInfosFilter), typeof(INonChangingCultureInfosFilter))]
[Register(typeof(CreateCultureInfosFilter), typeof(ICreateCultureInfosFilter))]
    
[RegisterModule(typeof(StandardModule))]
internal partial class StrongInjectContainer : IAsyncContainer<IContext>
{
    [Instance] private IActionInputs ActionInputs { get; }

    [Factory] private Regex CreateRegex(string pattern) => new (pattern);

    public StrongInjectContainer(
        IActionInputs actionInputs) =>
        ActionInputs = actionInputs;
}