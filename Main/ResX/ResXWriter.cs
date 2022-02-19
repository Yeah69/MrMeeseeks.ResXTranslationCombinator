using System.IO;
using System.Xml.Linq;
using MrMeeseeks.ResXTranslationCombinator.Utility;

namespace MrMeeseeks.ResXTranslationCombinator.ResX;

public interface IResXWriter
{
    void AddResource(IResXNode node);
    void Generate();
}

internal class ResXWriter : IResXWriter
{
    private readonly FileInfo _file;
    private readonly IResXElementsFactory _resXElementsFactory;
    private readonly ILogger _logger;
    private readonly XElement _root;

    public ResXWriter(
        // parameters
        FileInfo file,
        IResXElementsFactory resXElementsFactory,
            
        // dependencies
        ILogger logger)
    {
        _file = file;
        _resXElementsFactory = resXElementsFactory;
        _logger = logger;

        _root = resXElementsFactory.CreateNewRoot();
    }


    public void AddResource(IResXNode node)
    {
        var data = _resXElementsFactory.CreateNewDataElement();
        data.Attribute("name")?.SetValue(node.Name);
        data.Add(new XElement("value", node.Value));
        if (!string.IsNullOrWhiteSpace(node.Comment))
            data.Add(new XElement("comment", node.Comment));
            
        _root.Add(data);
    }

    public void Generate()
    {
        var xDocument = new XDocument(_root);
        xDocument.Save(_file.FullName);
        _logger.Notice(_file,$"Saving ResX file");
    }
}