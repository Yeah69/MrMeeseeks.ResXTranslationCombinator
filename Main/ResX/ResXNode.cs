namespace MrMeeseeks.ResXTranslationCombinator.ResX
{
    public interface IResXNode
    {
        string Name { get; }
        string Value { get; }
        string Comment { get; }
    }

    internal record ResXNode(
        string Name,
        string Value,
        string Comment) : IResXNode;
}