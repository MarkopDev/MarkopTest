using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Common;

namespace Application.Contracts.Persistence;

public interface IUnitOfWork: IDisposable
{
    int SaveChanges();
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntityBase;
    TRepository Repository<TRepository, TEntity>() where TRepository : class, IRepository<TEntity> where TEntity : class, IEntityBase;
}