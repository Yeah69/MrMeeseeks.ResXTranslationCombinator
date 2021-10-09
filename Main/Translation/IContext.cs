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
        private readonly IResXCombinator<IDeepLTranslator> _deepLCombinator;
        private readonly IResXCombinator<ICopyTranslator> _copyCombinator;
        private readonly ILogger _logger;
        private readonly Func<string, FileInfo> _fileInfoFactory;
        private readonly Func<string, DirectoryInfo> _directoryInfoFactory;
        private readonly Regex? _excludesRegex;
        private readonly Regex? _dataCopiesRegex;

        public DeepLContext(
            IActionInputs actionInputs,
            IResXCombinator<IDeepLTranslator> deepLCombinator,
            IResXCombinator<ICopyTranslator> copyCombinator,
            ILogger logger,
            Func<string, FileInfo> fileInfoFactory,
            Func<string, DirectoryInfo> directoryInfoFactory,
            Func<string, Regex> regexFactory)
        {
            _actionInputs = actionInputs;
            _deepLCombinator = deepLCombinator;
            _copyCombinator = copyCombinator;
            _logger = logger;
            _fileInfoFactory = fileInfoFactory;
            _directoryInfoFactory = directoryInfoFactory;
            
            if (actionInputs.ToString() is {} message)
                _logger.FileLessNotice(message);
            _excludesRegex = string.IsNullOrWhiteSpace(actionInputs.ExcludesRegex) ? null : regexFactory(actionInputs.ExcludesRegex);
            _dataCopiesRegex = string.IsNullOrWhiteSpace(actionInputs.DataCopiesRegex) ? null : regexFactory(actionInputs.DataCopiesRegex);
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
            {
                await (_dataCopiesRegex?.IsMatch(defaultResXFile.Name) ?? false 
                    ? _copyCombinator.Translate(defaultResXFile) 
                    : _deepLCombinator.Translate(defaultResXFile))
                    .ConfigureAwait(false);
            }

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
                if(!ret) _logger.Notice(defaultResXFile, "Default file excluded by regex");
                return ret;
            }
        }
    }
}