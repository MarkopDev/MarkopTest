using MediatR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Features.Account.Commands.SignUp;
using Application.Features.Account.Commands.SignIn;
using Application.Features.Account.Commands.SignOut;

namespace WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class AccountController : ControllerBase
{
    public AccountController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp(SignUpCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SignInViewModel), 200)]
    public async Task<IActionResult> SignIn(SignInCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpGet]
    public async Task<IActionResult> SignOut()
    {
        await Mediator.Send(new SignOutCommand());
        return Ok();
    }
}