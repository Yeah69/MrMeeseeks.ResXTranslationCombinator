using System;

namespace MrMeeseeks.ResXCombinator.Translation
{
    public class ResXTranslatorTranslationFailedException : Exception
    {
        public ResXTranslatorTranslationFailedException(
            Exception innerException) : base("Translation failed.", innerException)
        {
        }
    }
}