using System;
using System.IO;
using System.Reactive.Disposables;

namespace MrMeeseeks.ResXTranslationCombinator.Utility
{
    public interface ILogger
    {
        public void Notice(FileInfo file, string message);
        public void Warning(FileInfo file, string message);
        public void Error(FileInfo file, string message);
        void FileLessNotice(string message);
        void FileLessWarning(string message);
        void FileLessError(string message);

        void SetOutput(string name, string value);

        IDisposable Group(string title);
    }

    /// <summary>
    /// Github Action logger: 
    /// https://docs.github.com/actions/reference/workflow-commands-for-github-actions
    /// </summary>
    internal class Logger : ILogger
    {
        private const string NoticeLabel = "NTC";
        private const string WarningLabel = "WRN";
        private const string ErrorLabel = "ERR";
        private const string NoticeCommand = "notice";
        private const string WarningCommand = "warning";
        private const string ErrorCommand = "error";
        
        public void Notice(FileInfo file, string message) => Inner(NoticeCommand, file, NoticeLabel ,message);
        public void Warning(FileInfo file, string message) => Inner(WarningCommand, file, WarningLabel ,message);
        public void Error(FileInfo file, string message) => Inner(ErrorCommand, file, ErrorLabel ,message);
        private static void Inner(string command, FileSystemInfo file, string label, string message) => Log($"::{command} file={file.FullName}::[{label}] {message}");
        
        public void FileLessNotice(string message) => FileLessInner(NoticeLabel, message);
        public void FileLessWarning(string message) => FileLessInner(WarningLabel,message);
        public void FileLessError(string message) => FileLessInner(ErrorLabel, message);
        private static void FileLessInner(string label, string message) => Log($"[{label}] {message}");

        public void SetOutput(string name, string value) => Log($"::set-output name={name}::{value}");
        
        public IDisposable Group(string title)
        {
            Log($"::group::{title}");
            return Disposable.Create(() => Log("::endgroup::"));
        }

        private static void Log(string command) => Console.WriteLine(command);
    }
}