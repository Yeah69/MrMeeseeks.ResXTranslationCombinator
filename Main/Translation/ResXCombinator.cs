using System.IO;
using System.Threading.Tasks;
using MoreLinq;
using MrMeeseeks.ResXTranslationCombinator.ResX;
using MrMeeseeks.ResXTranslationCombinator.Utility;

namespace MrMeeseeks.ResXTranslationCombinator.Translation;

internal interface IResXCombinator<T> where T : ITranslator
{
    Task Translate(FileInfo defaultResXFile);
}

public enum ResXFileType
{
    AutomaticallyTranslated,
    ManuallyOverriden
}

internal class ResXCombinator<T> : IResXCombinator<T> where T : ITranslator
{
    private readonly ITranslator _translator;
    private readonly Func<FileInfo, IResXWriterFactory> _resXWriterFactoryFactory;
    private readonly Func<(string Name, string Value, string Comment), IResXNode> _resXNodeFactory;
    private readonly Func<string, FileInfo> _fileInfoFactory;
    private readonly IDataMappingFactory _dataMappingFactory;
    private readonly ILogger _logger;
    private readonly ICultureInfosFilter _cultureInfosFilter;

    public ResXCombinator(
        T translator,
        ICreateCultureInfosFilter createCultureInfosFilter,
        Func<FileInfo, IResXWriterFactory> resXWriterFactoryFactory,
        Func<(string Name, string Value, string Comment), IResXNode> resXNodeFactory,
        Func<string, FileInfo> fileInfoFactory,
        IDataMappingFactory dataMappingFactory,
        ILogger logger)
    {
        _translator = translator;
        _resXWriterFactoryFactory = resXWriterFactoryFactory;
        _resXNodeFactory = resXNodeFactory;
        _fileInfoFactory = fileInfoFactory;
        _dataMappingFactory = dataMappingFactory;
        _logger = logger;

        _cultureInfosFilter = createCultureInfosFilter.Create();
    }
        
    public async Task Translate(FileInfo defaultResXFile)
    {
        using var _ = _logger.Group($"Generating localizations for file: {defaultResXFile.FullName}");
        if (!defaultResXFile.Exists)
            throw new Exception();

        var placeholder = _resXWriterFactoryFactory(defaultResXFile);
            
        var dataMapping = _dataMappingFactory.Create(defaultResXFile);
            
        // Don't generate files where no texts are translated
        if (!dataMapping.Keys.Any()) return;

        var orderedDefaultKeys = dataMapping.Keys.ToImmutableSortedSet();

        var supportedCultureInfos = _cultureInfosFilter
            .Filter(await _translator.GetSupportedCultureInfos().ConfigureAwait(false));

        IReadOnlyList<KeyValuePair<string, string>> defaultKeyValuePairsToTranslate = dataMapping
            .Default
            .Where(kvp => orderedDefaultKeys.Contains(kvp.Key))
            .ToList();
            
        // Update automatics
        foreach (var supportedCultureInfo in supportedCultureInfos)
        {
                
            var mapping = dataMapping.Automatics.TryGetValue(supportedCultureInfo, out var val)
                ? val
                : ImmutableDictionary<string, string>.Empty;

            var acc = mapping;

            var areThereAddition = false;

            foreach (var batch in defaultKeyValuePairsToTranslate
                         .Where(d => !mapping.ContainsKey(d.Key))
                         .Batch(50) // Todo the fifties batching is a DeepL specific it would make more sense to move it to the DeepLTranslator implementation
                         .Select(b => b.ToArray()))
            {
                var translatedValues = await _translator.Translate(batch.Select(b => b.Value).ToArray(), supportedCultureInfo).ConfigureAwait(false);
                acc = acc.AddRange(batch.Zip(translatedValues, (b, t) => new KeyValuePair<string, string>(b.Key, t)));
                areThereAddition = true;
            }

            if (areThereAddition)
            {
                dataMapping.Automatics = dataMapping.Automatics.Remove(supportedCultureInfo).Add(supportedCultureInfo, acc);
                    
                var file = _fileInfoFactory(Path.Combine(defaultResXFile.DirectoryName ?? "",
                    $"{defaultResXFile.Name[..defaultResXFile.Name.IndexOf('.')]}.{supportedCultureInfo.Name}.a{defaultResXFile.Extension}"));
                _logger.Notice(file, "New translations added");

                if (_translator.TranslationsShouldBeCached)
                {
                    var resXResourceWriter = placeholder.Create(file);
                    foreach (var keyValuePair in acc.OrderBy(kvp => kvp.Key))
                    {
                        var resXDataNode = _resXNodeFactory((
                            keyValuePair.Key, 
                            keyValuePair.Value,
                            "Automatically Translated"));
                        resXResourceWriter.AddResource(resXDataNode);
                    }
                    resXResourceWriter.Generate();
                }
            }
        }
            
        // Generate combined ResX files
        foreach (var cultureInfo in supportedCultureInfos.Concat(dataMapping.Overrides.Select(kvp => kvp.Key)).Distinct())
        {
            var resXResourceWriter = placeholder.Create(
                _fileInfoFactory(Path.Combine(defaultResXFile.DirectoryName ?? "", 
                    $"{defaultResXFile.Name[..defaultResXFile.Name.IndexOf('.')]}.{cultureInfo.Name}{defaultResXFile.Extension}")));

            var overrideMapping = dataMapping.Overrides.TryGetValue(cultureInfo, out var o)
                ? o
                : ImmutableDictionary<string, string>.Empty;

            var automaticMapping = dataMapping.Automatics.TryGetValue(cultureInfo, out var a)
                ? a
                : ImmutableDictionary<string, string>.Empty;
                
            foreach (var key in orderedDefaultKeys)
            {
                string value = "";
                string comment = "Neither a manually overriden nor an automatically translated value found";
                    
                if (overrideMapping.TryGetValue(key, out var oVal))
                {
                    value = oVal;
                    comment = "Manually overriden.";
                }
                else if (automaticMapping.TryGetValue(key, out var aVal))
                {
                    value = aVal;
                    comment = "Automatically translated.";
                }
                    
                var resXDataNode = _resXNodeFactory((key, value, comment));
                resXResourceWriter.AddResource(resXDataNode);
            }
            resXResourceWriter.Generate();
        }
            
        foreach (var overrideItem in dataMapping.Overrides)
        {
            var resXResourceWriter = placeholder.Create(
                _fileInfoFactory(Path.Combine(defaultResXFile.DirectoryName ?? "", 
                    $"{defaultResXFile.Name[..defaultResXFile.Name.IndexOf('.')]}.{overrideItem.Key.Name}.o{defaultResXFile.Extension}")));
            foreach (var key in orderedDefaultKeys)
            {
                var resXDataNode = _resXNodeFactory((key, overrideItem.Value.TryGetValue(key, out var value) ? value : "", ""));
                resXResourceWriter.AddResource(resXDataNode);
            }
            resXResourceWriter.Generate();
        }
            
        var templateResXResourceWriter = placeholder.Create(
            _fileInfoFactory(Path.Combine(defaultResXFile.DirectoryName ?? "", 
                $"{defaultResXFile.Name[..defaultResXFile.Name.IndexOf('.')]}.template.o{defaultResXFile.Extension}")));
                
        foreach (var key in orderedDefaultKeys)
        {
            var resXDataNode = _resXNodeFactory((key, "", ""));
            templateResXResourceWriter.AddResource(resXDataNode);
        }
        templateResXResourceWriter.Generate();
    }
}