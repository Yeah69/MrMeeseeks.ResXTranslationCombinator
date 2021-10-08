using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public DeepLContext(
            IActionInputs actionInputs,
            IResXTranslator resXTranslator)
        {
            _actionInputs = actionInputs;
            _resXTranslator = resXTranslator;
        }
        
        public async Task TraverseAndTranslate()
        {
            var rootDirectory = new DirectoryInfo(_actionInputs.Directory);
            if (!rootDirectory.Exists)
            {
                Console.WriteLine("[Warning] Given root directory doesn't exist! Aborting!");
                return;
            }
            
            Directory.SetCurrentDirectory(rootDirectory.FullName);

            var defaultResXFiles = Directory.EnumerateFiles(".", "*.resx", SearchOption.AllDirectories)
                .Select(p => new FileInfo(p))
                .Where(fi => fi.Name.Split('.').Length == 2)
                .ToList();
    
            foreach (var defaultResXFile in defaultResXFiles)
            {
                await _resXTranslator.Translate(defaultResXFile.FullName).ConfigureAwait(false);
            }

            const string title = "Localization";
            const string summary = "Summary";

            // https://docs.github.com/actions/reference/workflow-commands-for-github-actions#setting-an-output-parameter
            Console.WriteLine($"::set-output name=summary-title::{title}");
            Console.WriteLine($"::set-output name=summary-details::{summary}");
        }
    }
}