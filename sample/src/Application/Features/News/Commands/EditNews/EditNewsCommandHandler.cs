using System.Threading;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Contracts.Persistence;
using Application.DTOs.News;
using AutoMapper;
using MediatR;

namespace Application.Features.News.Commands.EditNews;

public class EditNewsCommandHandler : IRequestHandler<EditNewsCommand, EditNewsViewModel>
{
    private IMapper Mapper { get; }
    private IUnitOfWork UnitOfWork { get; }

    public EditNewsCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        Mapper = mapper;
        UnitOfWork = unitOfWork;
    }
        
    public async Task<EditNewsViewModel> Handle(EditNewsCommand request, CancellationToken cancellationToken)
    {
        var news = UnitOfWork.Repository<Domain.Entities.News>()
            .Where(s => s.Id == request.NewsId)
            .Include(s => s.Author)
            .FirstOrDefault();

        if (news == null)
            throw new ServiceException(new Error
            {
                Code = ErrorCode.NotFound,
                Message = "News not found."
            });

        news.Title = request.Title;
        news.Content = request.Content;
        news.Preview = request.Preview;
        news.IsHidden = request.IsHidden;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return new EditNewsViewModel
        {
            News = Mapper.Map<NewsDto>(news)
        };
    }
}