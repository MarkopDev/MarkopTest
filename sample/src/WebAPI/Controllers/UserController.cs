using MediatR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Application.Features.User.Queries.GetProfile;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        public UserController(IMediator mediator) : base(mediator)
        {
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(GetProfileViewModel), 200)]
        public async Task<IActionResult> GetProfile(GetProfileQuery request)
        {
            return Ok(await Mediator.Send(request));
        }
    }
}