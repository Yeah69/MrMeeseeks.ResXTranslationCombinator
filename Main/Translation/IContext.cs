namespace MrMeeseeks.ResXTranslationCombinator.Translation
{
    public interface IContext
    {
        IResXTranslator Translator { get; }
    }
    
    public static class ContextFactory
    {
        public static IContext CreateDeepLContext(string authKeyForDeepLPro) => 
            new DeepLContext(authKeyForDeepLPro);
    }

    internal class DeepLContext : IContext
    {
        public DeepLContext(string authKeyForDeepLPro) =>
            Translator = new ResXTranslator(
                new DeepLTranslator(
                    authKeyForDeepLPro));

        public IResXTranslator Translator { get; }
    }
}