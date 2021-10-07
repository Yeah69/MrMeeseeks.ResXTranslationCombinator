using System;
using System.Linq;
using System.Xml.Linq;

namespace MrMeeseeks.ResXTranslationCombinator.ResX
{
    public interface IResXWriterFactory
    {
        IResXWriter Create(string path);
    }

    internal class ResXWriterFactory : IResXWriterFactory
    {
        private readonly Func<XElement> _dataFactory;
        private readonly Func<XElement> _rootFactory;

        public ResXWriterFactory(string pathOriginal)
        {
            var xDocument = XDocument.Load(pathOriginal);
            var root = xDocument.Root ?? throw new Exception("No root node"); 
            var dataElement = root.Element("data") ?? throw new Exception("No data element");
            dataElement.RemoveNodes();
            var xElements = root.Elements().Where(e => e.Name != "data").ToList();
            root.RemoveNodes();
            foreach (var xElement in xElements)
                root.Add(xElement);

            _rootFactory = () => new XElement(root);
            _dataFactory = () => new XElement(dataElement);
        }

        public IResXWriter Create(string path) => new ResXWriter(path, _rootFactory, _dataFactory);
    }
}