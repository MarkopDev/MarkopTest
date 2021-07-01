using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public IMemoryCache MemoryCache { get; }

        public UserRepository(DatabaseContext dbContext, IMemoryCache memoryCache) : base(dbContext)
        {
            MemoryCache = memoryCache;
        }

        public async Task<string[]> GetUserRoles(string userId)
        {
            var cacheKey = "UserRoles:" + userId;
            
            var userRoles = await MemoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(7);
                var roles = await _dbContext.UserRoles
                    .Join(_dbContext.Roles, userRole => userRole.RoleId, role => role.Id,
                        (userRole, role) => new {userRole, role})
                    .Where(@t => @t.userRole.UserId.Equals(userId))
                    .Select(@t => @t.role.Name).ToArrayAsync();
                
                return roles ?? Array.Empty<string>();
            });

            return userRoles;
        }
    }
}