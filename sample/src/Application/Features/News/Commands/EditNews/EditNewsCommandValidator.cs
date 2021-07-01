using FluentValidation;

namespace Application.Features.News.Commands.EditNews
{
    public class EditNewsCommandValidator : AbstractValidator<EditNewsCommand>
    {
        public EditNewsCommandValidator()
        {
            RuleFor(p => p.Title)
                .NotEmpty()
                .WithMessage("Title required.");
            RuleFor(p => p.IsHidden)
                .NotNull()
                .WithMessage("IsHidden required.");
            RuleFor(p => p.Content)
                .NotNull()
                .WithMessage("Content required.");
            RuleFor(p => p.Preview)
                .NotNull()
                .WithMessage("Preview required.");
            RuleFor(p => p.NewsId)
                .GreaterThanOrEqualTo(1)
                .WithMessage("News id required.");
        }
    }
}