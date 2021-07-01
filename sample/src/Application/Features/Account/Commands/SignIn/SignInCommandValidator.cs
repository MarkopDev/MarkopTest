using FluentValidation;

namespace Application.Features.Account.Commands.SignIn
{
    public class SignInCommandValidator : AbstractValidator<SignInCommand>
    {
        public SignInCommandValidator()
        {
            RuleFor(p => p.Login)
                .NotEmpty()
                .WithMessage("Invalid login type.");

            RuleFor(p => p.Password)
                .NotEmpty()
                .WithMessage("Password required.")
                .MinimumLength(8)
                .WithMessage("Password must greater than required.");
        }
    }
}