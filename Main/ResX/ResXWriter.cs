using System.Xml.Linq;

namespace MrMeeseeks.ResXTranslationCombinator.ResX
{
    public interface IResXWriter
    {
        void AddResource(IResXNode node);
        void Generate();
    }

    internal class ResXWriter : IResXWriter
    {
        private readonly string _path;
        private readonly IResXElementsFactory _resXElementsFactory;
        private readonly XElement _root;

        public ResXWriter(
            string path,
            IResXElementsFactory resXElementsFactory)
        {
            _path = path;
            _resXElementsFactory = resXElementsFactory;

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
            xDocument.Save(_path);
        }
    }
}