using MediatR;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.DTOs.News;
using Microsoft.AspNetCore.Mvc;
using Application.Features.News.Queries.GetNews;
using Application.Features.News.Queries.GetNewsListFast;
using Application.Features.News.Queries.GetNewsListSlow;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class NewsController : ControllerBase
{
    public NewsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaginationViewModel<NewsListItemDto>), 200)]
    public async Task<IActionResult> GetNewsListSlow(GetNewsListSlowQuery request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaginationViewModel<NewsListItemDto>), 200)]
    public async Task<IActionResult> GetNewsListFast(GetNewsListFastQuery request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpPost]
    [ProducesResponseType(typeof(GetNewsViewModel), 200)]
    public async Task<IActionResult> GetNews(GetNewsQuery request)
    {
        return Ok(await Mediator.Send(request));
    }
}