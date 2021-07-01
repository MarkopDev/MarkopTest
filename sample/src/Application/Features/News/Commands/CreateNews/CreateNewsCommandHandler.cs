using System.Threading;
using System.Threading.Tasks;
using Application.Contracts.Persistence;
using Application.DTOs.News;
using Application.Utilities;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.News.Commands.CreateNews
{
    public class CreateNewsCommandHandler : IRequestHandler<CreateNewsCommand, CreateNewsViewModel>
    {
        private IMapper Mapper { get; }
        private IUnitOfWork UnitOfWork { get; }
        private IHttpContextAccessor HttpContextAccessor { get; }

        public CreateNewsCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            Mapper = mapper;
            UnitOfWork = unitOfWork;
            HttpContextAccessor = httpContextAccessor;
        }


        public async Task<CreateNewsViewModel> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
        {
            var user = HttpContextAccessor.GetUser();

            var news = new Domain.Entities.News
            {
                Content = "",
                Preview = "",
                Author = user,
                IsHidden = true,
                Title = request.Title,
            };

            await UnitOfWork.Repository<Domain.Entities.News>().AddAsync(news);
            
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateNewsViewModel
            {
                News = Mapper.Map<NewsDto>(news)
            };
        }
    }
}