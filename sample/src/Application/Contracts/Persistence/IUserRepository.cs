using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Contracts.Persistence;

public interface IUserRepository : IRepository<User>
{
    public Task<string[]> GetUserRoles(string userId);
}