using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Contracts.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Identity;

public class AuthorizationHandler : AuthorizationHandler<AuthorizationRequirements>
{
    private IMemoryCache Cache { get; }
    private IUnitOfWork UnitOfWork { get; }

    public AuthorizationHandler(IMemoryCache cache, IUnitOfWork unitOfWork)
    {
        Cache = cache;
        UnitOfWork = unitOfWork;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        AuthorizationRequirements requirement)
    {
        if (context.User.Identity is {IsAuthenticated: false})
            return;

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var userRoles = await Cache.GetOrCreateAsync("UserRoles:" + userId, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await UnitOfWork.Repository<IUserRepository, User>().GetUserRoles(userId);
        });
        var isEnable = Cache.GetOrCreate("UserIsEnable:" + userId, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return UnitOfWork.Repository<User>().AsNoTracking(true)
                .Where(u => u.Id == userId).FirstOrDefault()
                ?.IsEnable;
        });

        if (isEnable == true && requirement.RoleNames.Intersect(userRoles).ToList().Count != 0)
            context.Succeed(requirement);
    }
}