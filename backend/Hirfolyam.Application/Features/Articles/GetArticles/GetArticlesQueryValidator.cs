using FluentValidation;

namespace Hirfolyam.Application.Features;

public sealed class GetArticlesQueryValidator : AbstractValidator<GetArticlesQuery>
{
    public GetArticlesQueryValidator()
    {
        RuleFor(query => query.Category).MaximumLength(64);
        RuleFor(query => query.ExcludedCategories)
            .Must(categories => categories is null || categories.Length <= 20)
            .WithMessage("Exclude no more than 20 categories.");
        RuleForEach(query => query.ExcludedCategories).MaximumLength(64);
        RuleFor(query => query.Source).MaximumLength(100);
        RuleFor(query => query.Sources)
            .Must(sources => sources is null || sources.Length <= 100)
            .WithMessage("Select no more than 100 sources.");
        RuleForEach(query => query.Sources).MaximumLength(100);
        RuleFor(query => query.Ids)
            .Must(ids => ids is null || ids.Length <= 100)
            .WithMessage("Request no more than 100 articles.");
        RuleFor(query => query.Search).MaximumLength(2000);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);

        RuleFor(query => query.Page)
            .Must((query, page) => ((long)page - 1) * query.PageSize <= int.MaxValue)
            .WithMessage("The page offset is too large.");
    }
}
