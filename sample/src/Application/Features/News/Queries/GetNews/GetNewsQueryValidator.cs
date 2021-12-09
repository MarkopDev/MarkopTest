using FluentValidation;

namespace Application.Features.News.Queries.GetNews;

public class GetNewsQueryValidator : AbstractValidator<GetNewsQuery>
{
    public GetNewsQueryValidator()
    {
        RuleFor(p => p.NewsId)
            .GreaterThanOrEqualTo(1)
            .WithMessage("News id required.");
    }
}