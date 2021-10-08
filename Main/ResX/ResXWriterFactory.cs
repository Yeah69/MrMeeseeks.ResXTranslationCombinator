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
        private readonly Func<string, IResXElementsFactory, IResXWriter> _resXWriterFactory;
        private readonly IResXElementsFactory _resxElementsFactoryFactory;

        public ResXWriterFactory(
            string pathOriginal,
            
            Func<(XElement RootTemplate, XElement DataTemplate), IResXElementsFactory> resXElementsFactoryFactory,
            Func<string, IResXElementsFactory, IResXWriter> resXWriterFactory)
        {
            _resXWriterFactory = resXWriterFactory;
            var xDocument = XDocument.Load(pathOriginal);
            var root = xDocument.Root ?? throw new Exception("No root node"); 
            var dataElement = root.Element("data") ?? throw new Exception("No data element");
            dataElement.RemoveNodes();
            var xElements = root.Elements().Where(e => e.Name != "data").ToList();
            root.RemoveNodes();
            foreach (var xElement in xElements)
                root.Add(xElement);
            
            _resxElementsFactoryFactory = resXElementsFactoryFactory((root, dataElement));
        }

        public IResXWriter Create(string path) => _resXWriterFactory(path, _resxElementsFactoryFactory);
    }
}