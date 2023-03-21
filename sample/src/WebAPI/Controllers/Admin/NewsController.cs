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

    [HttpPatch]
    [ProducesResponseType(typeof(EditNewsViewModel), 200)]
    public async Task<IActionResult> EditNews(EditNewsCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpPut]
    [ProducesResponseType(typeof(CreateNewsViewModel), 200)]
    public async Task<IActionResult> CreateNews(CreateNewsCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteNews([FromQuery] int newsId)
    {
        return Ok(await Mediator.Send(new DeleteNewsCommand { NewsId = newsId }));
    }
}