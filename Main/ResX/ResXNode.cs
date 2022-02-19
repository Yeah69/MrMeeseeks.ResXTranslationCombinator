namespace MrMeeseeks.ResXTranslationCombinator.ResX;

public interface IResXNode
{
    string Name { get; }
    string Value { get; }
    string Comment { get; }
}

internal record ResXNode : IResXNode
{
    public ResXNode((string Name, string Value, string Comment) tuple) => (Name, Value, Comment) = tuple;

    public string Name { get; }
    public string Value { get; }
    public string Comment { get; }
};