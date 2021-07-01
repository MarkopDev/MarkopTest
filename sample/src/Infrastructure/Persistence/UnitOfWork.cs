using System;
using System.Collections.Generic;
using Domain.Common;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Repositories;
using Application.Contracts.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        #region Private Fields

        private bool _disposed;
        private DatabaseContext _databaseContext;
        private Dictionary<string, dynamic> _repositories;
        private readonly IServiceProvider _serviceProvider;

        #endregion Private Fields

        #region Constuctor/Dispose

        public UnitOfWork(DatabaseContext databaseContext, IServiceProvider serviceProvider)
        {
            _databaseContext = databaseContext;
            _serviceProvider = serviceProvider;
            _repositories = new Dictionary<string, dynamic>();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_databaseContext != null)
            {
                _databaseContext.Dispose();
                _databaseContext = null;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion Constuctor/Dispose

        public int SaveChanges()
        {
            return _databaseContext.SaveChanges();
        }

        public Task<int> SaveChangesAsync()
        {
            return _databaseContext.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _databaseContext.SaveChangesAsync(cancellationToken);
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntityBase
        {
            if (_repositories == null)
                _repositories = new Dictionary<string, object>();

            var type = typeof(TEntity).Name;

            if (_repositories.ContainsKey(type))
                return (IRepository<TEntity>) _repositories[type];

            _repositories.Add(type, new RepositoryBase<TEntity>(_databaseContext));

            return _repositories[type];
        }

        public TRepository Repository<TRepository, TEntity>() where TRepository : class, IRepository<TEntity>
            where TEntity : class, IEntityBase
        {
            if (_repositories == null)
                _repositories = new Dictionary<string, object>();

            var type = typeof(TEntity).Name;

            if (_repositories.ContainsKey(type))
                return (TRepository) _repositories[type];

            var repository = _serviceProvider.GetService<TRepository>();
            
            _repositories.Add(type, repository);

            return _repositories[type];
        }
    }
}