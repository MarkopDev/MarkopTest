using MediatR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Application.Features.User.Queries.GetProfile;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class UserController : ControllerBase
{
    public UserController(IMediator mediator) : base(mediator)
    {
    }
        
    [HttpGet]
    [ProducesResponseType(typeof(GetProfileViewModel), 200)]
    public async Task<IActionResult> GetProfile()
    {
        return Ok(await Mediator.Send(new GetProfileQuery()));
    }
}