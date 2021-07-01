using System.Threading;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;

namespace Application.Features.News.Commands.DeleteNews
{
    public class DeleteNewsCommandHandler : IRequestHandler<DeleteNewsCommand>
    {
        private IMapper Mapper { get; }
        private IUnitOfWork UnitOfWork { get; }

        public DeleteNewsCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            Mapper = mapper;
            UnitOfWork = unitOfWork;
        }
        
        public async Task<Unit> Handle(DeleteNewsCommand request, CancellationToken cancellationToken)
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
            
            UnitOfWork.Repository<Domain.Entities.News>().Delete(news);
            
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}