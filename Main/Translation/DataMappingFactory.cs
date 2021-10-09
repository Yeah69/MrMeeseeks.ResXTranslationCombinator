using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using MrMeeseeks.ResXTranslationCombinator.ResX;
using MrMeeseeks.ResXTranslationCombinator.Utility;

namespace MrMeeseeks.ResXTranslationCombinator.Translation
{
    public interface IDataMappingFactory
    {
        IDataMapping Create(FileInfo defaultResXFile);
    }

    internal class DataMappingFactory : IDataMappingFactory
    {
        private readonly Func<FileInfo, IResXReader> _resXReaderFactory;
        private readonly Func<(IImmutableDictionary<string, string> Default, IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Automatics, IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Overrides), IDataMapping> _dataMappingFactory;
        private readonly Func<string, FileInfo> _fileInfoFactory;
        private readonly ILogger _logger;

        public DataMappingFactory(
            Func<FileInfo, IResXReader> resXReaderFactory,
            Func<(IImmutableDictionary<string, string> Default,
                IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Automatics,
                IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Overrides), IDataMapping> dataMappingFactory,
            Func<string, FileInfo> fileInfoFactory,
            ILogger logger)
        {
            _resXReaderFactory = resXReaderFactory;
            _dataMappingFactory = dataMappingFactory;
            _fileInfoFactory = fileInfoFactory;
            _logger = logger;
        }

        public IDataMapping Create(FileInfo defaultResXFile)
        {
            _logger.Notice(defaultResXFile, "Creating initial data mapping for the default file and the automatic and override files");
            var reader = _resXReaderFactory(defaultResXFile);

            var defaultResXFileNameWithoutExtension = defaultResXFile.Name[..defaultResXFile.Name.IndexOf('.')];

            var defaults = GetKeyValuesFromReader(reader, true);

            var aAndO = Directory.GetFiles(defaultResXFile.DirectoryName!)
                .Select(p => _fileInfoFactory(p))
                .Select(fi => (fi.Name.Split('.'), fi))
                .Where(t =>
                {
                    var nameParts = t.Item1;
                    return nameParts.Length == 4
                           && nameParts[0] == defaultResXFileNameWithoutExtension
                           && DoesCultureExist(nameParts[1])
                           && (nameParts[2] == "a" || nameParts[2] == "o")
                           && $".{nameParts[3]}" == defaultResXFile.Extension;
                })
                .Select(t => (
                    CultureInfo: CultureInfo.GetCultureInfo(t.Item1[1]),
                    Type: t.Item1[2] == "a"
                        ? ResXFileType.AutomaticallyTranslated
                        : ResXFileType.ManuallyOverriden,
                    FileInfo: t.fi))
                .ToList();

            var automatics = AandOToDictionary(aAndO, ResXFileType.AutomaticallyTranslated, _resXReaderFactory);

            var overrides = AandOToDictionary(aAndO, ResXFileType.ManuallyOverriden, _resXReaderFactory);

            return _dataMappingFactory((defaults, automatics, overrides));
            
            static IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> AandOToDictionary(
                IEnumerable<(CultureInfo CultureInfo, ResXFileType Type, FileInfo FileInfo)> aAndO,
                ResXFileType filterType,
                Func<FileInfo, IResXReader> resXReaderFactory) =>
                aAndO
                    .Where(t => t.Type == filterType)
                    .Select(t =>
                    {
                        var reader = resXReaderFactory(t.FileInfo);
                        return (t.CultureInfo, Map: GetKeyValuesFromReader(reader, false));
                    })
                    .ToImmutableDictionary(t => t.CultureInfo, t => t.Map);
            
            static IImmutableDictionary<string, string> GetKeyValuesFromReader(IResXReader reader, bool isDefault) => 
                reader
                    .Select(rdn => (rdn.Name, rdn.Value))
                    .Where(t => isDefault || !string.IsNullOrWhiteSpace(t.Value))
                    .ToImmutableDictionary(t => t.Name, t => t.Value);
			
            // https://stackoverflow.com/a/16476935/4871837 Thanks
            static bool DoesCultureExist(string cultureName) => CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Any(culture => string.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}