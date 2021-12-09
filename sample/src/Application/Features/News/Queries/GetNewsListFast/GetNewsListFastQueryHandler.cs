using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Contracts.Persistence;
using Application.DTOs.News;
using Application.Utilities;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.News.Queries.GetNewsListFast;

public class GetNewsListFastQueryHandler : IRequestHandler<GetNewsListFastQuery, PaginationViewModel<NewsListItemDto>>
{
    private IMapper Mapper { get; }
    private IUnitOfWork UnitOfWork { get; }
    private IHttpContextAccessor HttpContextAccessor { get; }
    private UserManager<Domain.Entities.User> UserManager { get; }

    public GetNewsListFastQueryHandler(IUnitOfWork unitOfWork, IMapper mapper,
        UserManager<Domain.Entities.User> userManager, IHttpContextAccessor httpContextAccessor)
    {
        Mapper = mapper;
        UnitOfWork = unitOfWork;
        UserManager = userManager;
        HttpContextAccessor = httpContextAccessor;
    }

    public async Task<PaginationViewModel<NewsListItemDto>> Handle(GetNewsListFastQuery request,
        CancellationToken cancellationToken)
    {
        var user = HttpContextAccessor.GetUser();
        var isAdmin = await UserManager.IsInRoleAsync(user, "Owner")
                      || await UserManager.IsInRoleAsync(user, "Admin");

        var newsList = await UnitOfWork.Repository<Domain.Entities.News>()
            .Where(s => (s.IsHidden.HasValue && !s.IsHidden.Value) || isAdmin)
            .Where(news => string.IsNullOrWhiteSpace(request.Search) ||
                           EF.Functions.Like(news.Title.ToLower(), $"%{request.Search.ToLower()}%") ||
                           EF.Functions.Like(news.Content.ToLower(), $"%{request.Search.ToLower()}%") ||
                           EF.Functions.Like(news.Preview.ToLower(), $"%{request.Search.ToLower()}%") ||
                           EF.Functions.Like(news.Author.FirstName.ToLower(), $"%{request.Search.ToLower()}%") ||
                           EF.Functions.Like(news.Author.LastName.ToLower(), $"%{request.Search.ToLower()}%") ||
                           EF.Functions.Like(news.Author.LastName.ToLower(), $"%{request.Search.ToLower()}%"))
            .Include(s => s.Author)
            .OrderBy(queryable => queryable.OrderByDescending(s => s.CreatedDate))
            .GetPage<NewsListItemDto>(Mapper.ConfigurationProvider, request.PageNumber, request.PageSize, request.SortPropertyName, request.SortType);

        return newsList;
    }
}