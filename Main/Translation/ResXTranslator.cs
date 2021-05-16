using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using MoreLinq;

namespace MrMeeseeks.ResXCombinator.Translation
{
    public interface IResXTranslator
    {
        Task Translate(string pathToDefaultResXFile);
    }

    public enum ResXFileType
    {
        AutomaticallyTranslated,
        ManuallyOverriden
    }

    internal class ResXTranslator : IResXTranslator
    {
        private readonly ITranslator _translator;

        public ResXTranslator(
            ITranslator translator)
        {
            _translator = translator;
        }
        
        public async Task Translate(string pathToDefaultResXFile)
        {
            var defaultResourceFile = new FileInfo(pathToDefaultResXFile);
            if (!defaultResourceFile.Exists)
                throw new Exception();

            var (defaults, automatics, overrides) =
                GetMappings(pathToDefaultResXFile);

            var supportedCultureInfos = new HashSet<CultureInfo>(await _translator.GetSupportedCultureInfos().ConfigureAwait(false));
            
            
            // Update automatics
            foreach (var supportedCultureInfo in supportedCultureInfos)
            {
                
                var mapping = automatics.TryGetValue(supportedCultureInfo, out var val)
                    ? val
                    : ImmutableDictionary<string, string>.Empty;

                var acc = mapping;

                var areThereAddition = false;

                foreach (var batch in defaults.Where(d => !mapping.ContainsKey(d.Key)).Batch(50).Select(b => b.ToArray()))
                {
                    var translatedValues = await _translator.Translate(batch.Select(b => b.Value).ToArray(), supportedCultureInfo).ConfigureAwait(false);
                    acc = acc.AddRange(batch.Zip(translatedValues, (b, t) => new KeyValuePair<string, string>(b.Key, t)));
                    areThereAddition = true;
                }

                if (areThereAddition)
                {
                    automatics = automatics.Remove(supportedCultureInfo);
                    automatics = automatics.Add(supportedCultureInfo, acc);
                    using var resXResourceWriter = new ResXResourceWriter(
                        Path.Combine(defaultResourceFile.DirectoryName ?? "", 
                            $"{defaultResourceFile.Name[..defaultResourceFile.Name.IndexOf('.')]}.{supportedCultureInfo.Name}.a{defaultResourceFile.Extension}"));
                    foreach (var keyValuePair in acc)
                    {
                        var resXDataNode = new ResXDataNode(keyValuePair.Key, keyValuePair.Value)
                        {
                            Comment = "Automatically Translated"
                        };
                        resXResourceWriter.AddResource(resXDataNode);
                    }
                    resXResourceWriter.Generate();
                    resXResourceWriter.Close();
                }
            }
            
            // Generate combined ResX files
            foreach (var cultureInfo in supportedCultureInfos.Concat(overrides.Select(kvp => kvp.Key)).Distinct())
            {
                using var resXResourceWriter = new ResXResourceWriter(
                    Path.Combine(defaultResourceFile.DirectoryName ?? "", 
                        $"{defaultResourceFile.Name[..defaultResourceFile.Name.IndexOf('.')]}.{cultureInfo.Name}{defaultResourceFile.Extension}"));

                var overrideMapping = overrides.TryGetValue(cultureInfo, out var o)
                    ? o
                    : ImmutableDictionary<string, string>.Empty;

                var automaticMapping = automatics.TryGetValue(cultureInfo, out var a)
                    ? a
                    : ImmutableDictionary<string, string>.Empty;
                
                foreach (var key in defaults.Keys)
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
                    
                    var resXDataNode = new ResXDataNode(key, value)
                    {
                        Comment = comment
                    };
                    resXResourceWriter.AddResource(resXDataNode);
                }
                resXResourceWriter.Generate();
                resXResourceWriter.Close();
            }
        }

        private static (IImmutableDictionary<string, string> Default,
            IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Automatics,
            IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> Overrides) GetMappings(
                string pathToDefaultResXFile)
        {
            var defaultResourceFile = new FileInfo(pathToDefaultResXFile);
            if (!defaultResourceFile.Exists)
                throw new Exception();
            using var reader =
                new ResXResourceReader(defaultResourceFile.FullName)
                {
                    UseResXDataNodes = true
                };

            var defaultResXFileNameWithoutExtension = defaultResourceFile.Name[..defaultResourceFile.Name.IndexOf('.')];

            var defaults = GetKeyValuesFromReader(reader, true);

            var aAndO = Directory.GetFiles(defaultResourceFile.DirectoryName!)
                .Select(p => new FileInfo(p))
                .Select(fi => (fi.Name.Split('.'), fi))
                .Where(t =>
                {
                    var nameParts = t.Item1;
                    return nameParts.Length == 4
                           && nameParts[0] == defaultResXFileNameWithoutExtension
                           && DoesCultureExist(nameParts[1])
                           && (nameParts[2] == "a" || nameParts[2] == "o")
                           && $".{nameParts[3]}" == defaultResourceFile.Extension;
                })
                .Select(t => (
                    CultureInfo: CultureInfo.GetCultureInfo(t.Item1[1]),
                    Type: t.Item1[2] == "a"
                        ? ResXFileType.AutomaticallyTranslated
                        : ResXFileType.ManuallyOverriden,
                    FileInfo: t.fi))
                .ToList();

            var automatics = AandOToDictionary(aAndO, ResXFileType.AutomaticallyTranslated);

            var overrides = AandOToDictionary(aAndO, ResXFileType.ManuallyOverriden);

            return (defaults, automatics, overrides);
            
            static IImmutableDictionary<CultureInfo, IImmutableDictionary<string, string>> AandOToDictionary(
                IEnumerable<(CultureInfo CultureInfo, ResXFileType Type, FileInfo FileInfo)> aAndO,
                ResXFileType filterType) =>
                aAndO
                    .Where(t => t.Type == filterType)
                    .Select(t =>
                    {
                        using var reader =
                            new ResXResourceReader(t.FileInfo.FullName)
                            {
                                UseResXDataNodes = true
                            };
                        return (t.CultureInfo, Map: GetKeyValuesFromReader(reader, false));
                    })
                    .ToImmutableDictionary(t => t.CultureInfo, t => t.Map);
            
            static IImmutableDictionary<string, string> GetKeyValuesFromReader(ResXResourceReader reader, bool isDefault) => 
                reader.Cast<DictionaryEntry>()
                    .Select(de => de.Value)
                    .Cast<ResXDataNode>()
                    .Select(rdn => (Key: rdn.Name, Value: rdn.GetValue((ITypeResolutionService?) null)?.ToString() ?? ""))
                    .Where(t => isDefault || !string.IsNullOrWhiteSpace(t.Value))
                    .ToImmutableDictionary(t => t.Key, t => t.Value);
			
            // https://stackoverflow.com/a/16476935/4871837 Thanks
            static bool DoesCultureExist(string cultureName) => CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Any(culture => string.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}