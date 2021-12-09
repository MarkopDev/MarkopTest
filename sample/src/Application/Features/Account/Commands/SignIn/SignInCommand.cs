using Application.Common.Enums;
using MediatR;

namespace Application.Features.Account.Commands.SignIn;

public class SignInCommand : IRequest<SignInViewModel>
{
    public string Login { get; set; }
    public LoginType Type { get; set; }
    public string Password { get; set; }
}