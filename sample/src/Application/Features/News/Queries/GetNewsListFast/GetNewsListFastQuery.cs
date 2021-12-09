using Application.Common.Models;
using Application.DTOs.News;

namespace Application.Features.News.Queries.GetNewsListFast;

public class GetNewsListFastQuery : PaginationRequest<NewsListItemDto>
{
    public string Search { get; set; }
}