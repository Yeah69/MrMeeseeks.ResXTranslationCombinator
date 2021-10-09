using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MrMeeseeks.ResXTranslationCombinator.Utility;

namespace MrMeeseeks.ResXTranslationCombinator.Translation
{
    public interface IContext
    {
        Task TraverseAndTranslate();
    }

    internal class DeepLContext : IContext
    {
        private readonly IActionInputs _actionInputs;
        private readonly IResXTranslator _resXTranslator;
        private readonly ILogger _logger;
        private readonly Func<string, FileInfo> _fileInfoFactory;
        private readonly Func<string, DirectoryInfo> _directoryInfoFactory;
        private readonly Regex? _excludesRegex;

        public DeepLContext(
            IActionInputs actionInputs,
            IResXTranslator resXTranslator,
            ILogger logger,
            Func<string, FileInfo> fileInfoFactory,
            Func<string, DirectoryInfo> directoryInfoFactory,
            Func<string, Regex> regexFactory)
        {
            _actionInputs = actionInputs;
            _resXTranslator = resXTranslator;
            _logger = logger;
            _fileInfoFactory = fileInfoFactory;
            _directoryInfoFactory = directoryInfoFactory;

            _excludesRegex = string.IsNullOrWhiteSpace(actionInputs.ExcludesRegex) ? null : regexFactory(actionInputs.ExcludesRegex) ;
        }
        
        public async Task TraverseAndTranslate()
        {
            var rootDirectory = _directoryInfoFactory(_actionInputs.Directory);
            if (!rootDirectory.Exists)
            {
                _logger.FileLessError("Given root directory doesn't exist! Aborting!");
                return;
            }
            
            _logger.FileLessNotice($"Setting current directory to: {rootDirectory.FullName}");
            Directory.SetCurrentDirectory(rootDirectory.FullName);

            foreach (var defaultResXFile in Directory.EnumerateFiles(".", "*.resx", SearchOption.AllDirectories)
                .Select(ToFileInfo)
                .Where(IsDefaultResxFileName)
                .Where(IsNotExcluded))
                await _resXTranslator.Translate(defaultResXFile).ConfigureAwait(false);

            _logger.SetOutput("summary-title", "Localization");
            _logger.SetOutput("summary-details", "Summary");
            
            // Local functions
            FileInfo ToFileInfo(string p) => _fileInfoFactory(p);
            bool IsDefaultResxFileName(FileInfo fi)
            {
                // a default ResX file has only one dot
                var ret = fi.Name.EndsWith(".resx") && fi.Name.Count(c => c == '.') == 1;
                if(ret) _logger.Notice(fi, "Default ResX file spotted");
                return ret;
            } 
            bool IsNotExcluded(FileInfo defaultResXFile)
            {
                var ret = !(_excludesRegex?.IsMatch(defaultResXFile.Name) ?? false);
                if(ret) _logger.Notice(defaultResXFile, "Default file excluded by regex");
                return ret;
            }
        }
    }
}