using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace MrMeeseeks.ResXTranslationCombinator.ResX
{
    public interface IResXReader : IEnumerable<IResXNode>
    {
        
    }

    internal class ResXReader : IResXReader
    {
        private readonly XDocument _xDocument;

        public ResXReader(string path) => _xDocument = XDocument.Load(path);

        public IEnumerator<IResXNode> GetEnumerator()
        {
            return _xDocument.XPathSelectElements("root/data")
                .Select(xe =>
                {
                    var name = xe.Attribute("name")?.Value ?? "";
                    var value = xe.Element("value")?.Value ?? "";
                    var comment = xe.Element("comment")?.Value ?? "";
                    return new ResXNode(name, value, comment);
                })
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}