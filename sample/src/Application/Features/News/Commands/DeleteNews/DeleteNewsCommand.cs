using MediatR;

namespace Application.Features.News.Commands.DeleteNews;

public class DeleteNewsCommand : IRequest
{
    public int NewsId { get; set; }
}