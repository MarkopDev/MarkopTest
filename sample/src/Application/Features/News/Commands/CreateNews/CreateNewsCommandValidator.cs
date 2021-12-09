using FluentValidation;

namespace Application.Features.News.Commands.CreateNews;

public class CreateNewsCommandValidator : AbstractValidator<CreateNewsCommand>
{
    public CreateNewsCommandValidator()
    {
        RuleFor(p => p.Title)
            .NotEmpty()
            .WithMessage("Title required.");
    }
}