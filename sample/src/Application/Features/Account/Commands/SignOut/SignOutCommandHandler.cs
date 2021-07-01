using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Account.Commands.SignOut
{
    public class SignOutCommandHandler : IRequestHandler<SignOutCommand>
    {
        private IHttpContextAccessor HttpContextAccessor { get; }
        private SignInManager<Domain.Entities.User> SignInManager { get; }

        public SignOutCommandHandler(IHttpContextAccessor httpContextAccessor, SignInManager<Domain.Entities.User> signInManager)
        {
            SignInManager = signInManager;
            HttpContextAccessor = httpContextAccessor;
        }


        public async Task<Unit> Handle(SignOutCommand request, CancellationToken cancellationToken)
        {
            await SignInManager.SignOutAsync();
            HttpContextAccessor.HttpContext?.Response.Headers.Remove("Roles");
            return Unit.Value;
        }
    }
}