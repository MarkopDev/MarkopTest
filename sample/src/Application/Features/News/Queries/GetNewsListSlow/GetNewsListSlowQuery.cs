using Application.Common.Models;
using Application.DTOs.News;

namespace Application.Features.News.Queries.GetNewsListSlow
{
    public class GetNewsListSlowQuery : PaginationRequest<NewsListItemDto>
    {
        public string Search { get; set; }
    }
}