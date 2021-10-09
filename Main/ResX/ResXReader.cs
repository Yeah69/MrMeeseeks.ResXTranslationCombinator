using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        private readonly Func<(string Name, string Value, string Comment), IResXNode> _resxNodeFactory;
        private readonly XDocument _xDocument;

        public ResXReader(
            // parameters
            FileInfo file,
            
            // dependencies
            Func<(string Name, string Value, string Comment), IResXNode> resxNodeFactory)
        {
            _resxNodeFactory = resxNodeFactory;
            _xDocument = XDocument.Load(file.FullName);
        }

        public IEnumerator<IResXNode> GetEnumerator()
        {
            return _xDocument.XPathSelectElements("root/data")
                .Select(xe =>
                {
                    var name = xe.Attribute("name")?.Value ?? "";
                    var value = xe.Element("value")?.Value ?? "";
                    var comment = xe.Element("comment")?.Value ?? "";
                    return _resxNodeFactory((name, value, comment));
                })
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}