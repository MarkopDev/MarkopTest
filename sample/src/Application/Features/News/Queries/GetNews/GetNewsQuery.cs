using MediatR;

namespace Application.Features.News.Queries.GetNews;

public class GetNewsQuery : IRequest<GetNewsViewModel>
{
    public int NewsId { get; set; }
}