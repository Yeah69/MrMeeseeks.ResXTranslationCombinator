using System.Xml.Linq;

namespace MrMeeseeks.ResXTranslationCombinator.ResX
{
    public interface IResXElementsFactory
    {
        XElement CreateNewRoot();
        XElement CreateNewDataElement();
    }

    internal class ResXElementsFactory : IResXElementsFactory
    {
        private readonly XElement _rootTemplate;
        private readonly XElement _dataTemplate;

        public ResXElementsFactory((XElement RootTemplate, XElement DataTemplate) tuple) => (_rootTemplate, _dataTemplate) = tuple;

        public XElement CreateNewRoot() => new (_rootTemplate);

        public XElement CreateNewDataElement() => new (_dataTemplate);
    }
}