namespace MrMeeseeks.ResXTranslationCombinator
{
    public interface IActionInputs
    {
        string Directory { get; }
        string AuthKey { get; }
        string ExcludesRegex { get; }
    }
}