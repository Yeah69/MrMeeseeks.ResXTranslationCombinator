using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MrMeeseeks.ResXTranslationCombinator.Utility;

namespace MrMeeseeks.ResXTranslationCombinator.ResX
{
    public interface IResXWriterFactory
    {
        IResXWriter Create(FileInfo file);
    }

    internal class ResXWriterFactory : IResXWriterFactory
    {
        private readonly Func<FileInfo, IResXElementsFactory, IResXWriter> _resXWriterFactory;
        private readonly IResXElementsFactory _resxElementsFactoryFactory;

        public ResXWriterFactory(
            // parameters
            FileInfo file,
            
            // dependencies
            Func<(XElement RootTemplate, XElement DataTemplate), IResXElementsFactory> resXElementsFactoryFactory,
            Func<FileInfo, IResXElementsFactory, IResXWriter> resXWriterFactory,
            ILogger logger)
        {
            _resXWriterFactory = resXWriterFactory;
            
            logger.Notice(file, "Creating ResX root- and data-template from default ResX file");
            var xDocument = XDocument.Load(file.FullName);
            var root = xDocument.Root ?? throw new Exception("No root node"); 
            var dataElement = root.Element("data") ?? throw new Exception("No data element");
            dataElement.RemoveNodes();
            var xElements = root.Elements().Where(e => e.Name != "data").ToList();
            root.RemoveNodes();
            foreach (var xElement in xElements)
                root.Add(xElement);
            
            _resxElementsFactoryFactory = resXElementsFactoryFactory((root, dataElement));
        }

        public IResXWriter Create(FileInfo file) => _resXWriterFactory(file, _resxElementsFactoryFactory);
    }
}