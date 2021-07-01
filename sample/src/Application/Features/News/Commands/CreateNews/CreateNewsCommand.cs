using MediatR;

namespace Application.Features.News.Commands.CreateNews
{
    public class CreateNewsCommand : IRequest<CreateNewsViewModel>
    {
        public string Title { get; set; }
    }
}