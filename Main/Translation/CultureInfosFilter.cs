using System.Globalization;

namespace MrMeeseeks.ResXTranslationCombinator.Translation;

internal interface ICultureInfosFilter
{
    IImmutableSet<CultureInfo> Filter(IImmutableSet<CultureInfo> unfiltered);
}

internal interface IIntersectingCultureInfosFilter : ICultureInfosFilter {}

internal class IntersectingCultureInfosFilter : IIntersectingCultureInfosFilter
{
    private readonly IImmutableSet<CultureInfo> _filter;

    public IntersectingCultureInfosFilter(
        IActionInputs actionInputs) =>
        _filter = ImmutableHashSet.CreateRange(actionInputs.LocalizationFilter.Split(';').Select(CultureInfo.GetCultureInfo));

    public IImmutableSet<CultureInfo> Filter(IImmutableSet<CultureInfo> unfiltered) => unfiltered.Intersect(_filter);
}

internal interface IExcludingCultureInfosFilter : ICultureInfosFilter {}

internal class ExcludingCultureInfosFilter : IExcludingCultureInfosFilter
{
    private readonly IImmutableSet<CultureInfo> _filter;

    public ExcludingCultureInfosFilter(
        IActionInputs actionInputs) =>
        _filter = ImmutableHashSet.CreateRange(actionInputs.LocalizationExcludes.Split(';').Select(CultureInfo.GetCultureInfo));

    public IImmutableSet<CultureInfo> Filter(IImmutableSet<CultureInfo> unfiltered) => unfiltered.Except(_filter);
}

internal interface INonChangingCultureInfosFilter : ICultureInfosFilter {}

internal class NonChangingCultureInfosFilter : INonChangingCultureInfosFilter
{
    public IImmutableSet<CultureInfo> Filter(IImmutableSet<CultureInfo> unfiltered) => unfiltered;
}

internal interface ICreateCultureInfosFilter
{
    ICultureInfosFilter Create();
}

internal class CreateCultureInfosFilter : ICreateCultureInfosFilter
{
    private readonly IActionInputs _actionInputs;
    private readonly Func<INonChangingCultureInfosFilter> _nonChangingCultureInfosFilterFactory;
    private readonly Func<IActionInputs, IIntersectingCultureInfosFilter> _intersectingCultureInfosFilterFactory;
    private readonly Func<IActionInputs, IExcludingCultureInfosFilter> _excludingCultureInfosFilterFactory;

    public CreateCultureInfosFilter(
        IActionInputs actionInputs,
        Func<INonChangingCultureInfosFilter> nonChangingCultureInfosFilterFactory,
        Func<IActionInputs, IIntersectingCultureInfosFilter> intersectingCultureInfosFilterFactory,
        Func<IActionInputs, IExcludingCultureInfosFilter> excludingCultureInfosFilterFactory)
    {
        _actionInputs = actionInputs;
        _nonChangingCultureInfosFilterFactory = nonChangingCultureInfosFilterFactory;
        _intersectingCultureInfosFilterFactory = intersectingCultureInfosFilterFactory;
        _excludingCultureInfosFilterFactory = excludingCultureInfosFilterFactory;
    }

    public ICultureInfosFilter Create()
    {
        if (_actionInputs.LocalizationFilter == string.Empty && _actionInputs.LocalizationExcludes == string.Empty)
            return _nonChangingCultureInfosFilterFactory();
        
        if (_actionInputs.LocalizationFilter != string.Empty && _actionInputs.LocalizationExcludes != string.Empty)
            throw new Exception("Using localization-filter and localization-excludes in combination is forbidden.");

        if (_actionInputs.LocalizationFilter != string.Empty)
            return _intersectingCultureInfosFilterFactory(_actionInputs);

        return _excludingCultureInfosFilterFactory(_actionInputs);
    }
}