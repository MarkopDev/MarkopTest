using FluentValidation;

namespace Application.Features.News.Commands.DeleteNews;

public class DeleteNewsCommandValidator : AbstractValidator<DeleteNewsCommand>
{
    public DeleteNewsCommandValidator()
    {
        RuleFor(p => p.NewsId)
            .GreaterThanOrEqualTo(1)
            .WithMessage("News id required.");
    }
}