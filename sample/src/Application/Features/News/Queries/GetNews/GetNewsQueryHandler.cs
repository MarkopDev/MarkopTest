using MediatR;
using AutoMapper;
using System.Linq;
using System.Threading;
using Application.DTOs.News;
using System.Threading.Tasks;
using System.Security.Claims;
using Application.Common.Enums;
using Application.Common.Models;
using Application.Common.Exceptions;
using Microsoft.AspNetCore.Identity;
using Application.Contracts.Persistence;
using Microsoft.AspNetCore.Http;

namespace Application.Features.News.Queries.GetNews;

public class GetNewsQueryHandler : IRequestHandler<GetNewsQuery, GetNewsViewModel>
{
    private IMapper Mapper { get; }
    private IUnitOfWork UnitOfWork { get; }
    private IHttpContextAccessor HttpContextAccessor { get; }
    private UserManager<Domain.Entities.User> UserManager { get; }
        
    public GetNewsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper,
        UserManager<Domain.Entities.User> userManager, IHttpContextAccessor httpContextAccessor)
    {
        Mapper = mapper;
        UnitOfWork = unitOfWork;
        UserManager = userManager;
        HttpContextAccessor = httpContextAccessor;
    }

    public async Task<GetNewsViewModel> Handle(GetNewsQuery request, CancellationToken cancellationToken)
    {
        var userId = HttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await UserManager.FindByIdAsync(userId);
        var isAdmin = await UserManager.IsInRoleAsync(user, "Owner")
                      || await UserManager.IsInRoleAsync(user, "Admin");
            
        var news = UnitOfWork.Repository<Domain.Entities.News>()
            .Where(s => s.Id == request.NewsId)
            .Where(s => (s.IsHidden.HasValue && !s.IsHidden.Value) || isAdmin)
            .Include(s => s.Author)
            .OrderBy(queryable => queryable.OrderByDescending(s => s.CreatedDate))
            .FirstOrDefault();

        if (news == null)
            throw new ServiceException(new Error
            {
                Code = ErrorCode.NotFound,
                Message = "News not found."
            });

        return new GetNewsViewModel
        {
            News = Mapper.Map<NewsDto>(news)
        };
    }
}