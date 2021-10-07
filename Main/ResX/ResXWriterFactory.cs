using System;
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
            foreach (var xElement in root.Elements("data"))
                xElement.Remove();

            _rootFactory = () => new XElement(root);
            _dataFactory = () => new XElement(dataElement);
        }

        public IResXWriter Create(string path) => new ResXWriter(path, _rootFactory, _dataFactory);
    }
}