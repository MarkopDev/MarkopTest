using Application.Utilities;
using FluentValidation;

namespace Application.Features.Account.Commands.SignUp
{
    public class SignUpCommandValidator : AbstractValidator<SignUpCommand>
    {
        public SignUpCommandValidator()
        {
            RuleFor(p => p.FirstName)
                .NotNull()
                .WithMessage("First name required.");

            RuleFor(p => p.LastName)
                .NotNull()
                .WithMessage("Last name required.");

            RuleFor(p => p.PhoneNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Phone number required.")
                .Must(s => s.IsPhoneNumber())
                .WithMessage("Invalid phone number.");

            RuleFor(p => p.Password)
                .NotEmpty()
                .WithMessage("Password required.")
                .MinimumLength(8)
                .WithMessage("Password must greater than required.");
        }
    }
}