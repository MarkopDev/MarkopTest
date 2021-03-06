using System.Threading.Tasks;
using Application.Features.News.Commands.CreateNews;
using Application.Features.News.Commands.DeleteNews;
using Application.Features.News.Commands.EditNews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "AdminOwnerPolicy")]
[Route("api/v{version:apiVersion}/Admin/[controller]/[action]")]
public class NewsController : ControllerBase
{
    public NewsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    [ProducesResponseType(typeof(EditNewsViewModel), 200)]
    public async Task<IActionResult> EditNews(EditNewsCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateNewsViewModel), 200)]
    public async Task<IActionResult> CreateNews(CreateNewsCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteNews(DeleteNewsCommand request)
    {
        return Ok(await Mediator.Send(request));
    }
}