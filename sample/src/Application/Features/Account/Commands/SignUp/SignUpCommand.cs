using MediatR;

namespace Application.Features.Account.Commands.SignUp;

public class SignUpCommand : IRequest<SignUpViewModel>
{
    public string Password { get; set; }
    public string LastName { get; set; }
    public string Platform { get; set; }
    public string FirstName { get; set; }
    public string PhoneNumber { get; set; }
}