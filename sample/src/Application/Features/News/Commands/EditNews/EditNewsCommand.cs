using MediatR;

namespace Application.Features.News.Commands.EditNews
{
    public class EditNewsCommand : IRequest<EditNewsViewModel>
    {
        public int NewsId { get; set; }
        public string Title { get; set; }
        public bool IsHidden { get; set; }
        public string Content { get; set; }
        public string Preview { get; set; }
    }
}