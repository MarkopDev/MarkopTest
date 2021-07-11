using System;
using System.Linq;
using Domain.Common;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Application.Common.Enums;
using Application.Common.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Contracts.Persistence;
using Application.Utilities;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Infrastructure.Repositories
{
    public class RepositoryBase<T> : IRepository<T> where T : class, IEntityBase
    {
        protected readonly DatabaseContext _dbContext;

        public RepositoryBase(DatabaseContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public IQueryable<T> Queryable(List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = true,
            bool filterDeletedItem = true)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (disableTracking)
                query = query.AsNoTracking();

            if (includes != null)
                query = includes.Aggregate(query, (current, include) => current.Include(include));

            if (predicates != null)
                query = predicates.Aggregate(query, (current, predicate) => current.Where(predicate));

            if (filterDeletedItem)
                query = query.Where(t => !t.IsDeleted);

            if (orderBys != null)
                query = orderBys.Aggregate(query, (current, orderBy) => orderBy(current));

            return query;
        }

        public async Task<IReadOnlyList<T>> Get(List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = true,
            bool filterDeletedItem = true)
        {
            return await Queryable(predicates, orderBys, includes, disableTracking, filterDeletedItem).ToListAsync();
        }

        public virtual async Task<PaginationViewModel<T>> GetPage(int pageNumber, int pageSize,
            string sortPropertyName = "", SortType? sortType = SortType.Ascending,
            List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = true,
            bool filterDeletedItem = true)
        {
            var query = Queryable(predicates, orderBys, includes, disableTracking, filterDeletedItem);

            /////////////////////////////////////////
            // This section has a performance issue
            // use following code to fix it
            // var count = await query.CountAsync();
            /////////////////////////////////////////
            var count = query.Count();
            var data = await query
                .OrderBy(sortPropertyName, sortType)
                .Skip(Math.Max(pageSize * (pageNumber - 1), 0)).Take(pageSize)
                .ToListAsync();

            return new PaginationViewModel<T>
            {
                Data = data,
                Total = count
            };
        }

        public virtual async Task<PaginationViewModel<S>> GetPage<S>(IConfigurationProvider configurationProvider,
            int pageNumber, int pageSize, string sortPropertyName = "", SortType? sortType = SortType.Ascending,
            List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = true,
            bool filterDeletedItem = true,
            bool queryMapping = true)
        {
            var query = Queryable(predicates, orderBys, includes, disableTracking, filterDeletedItem);

            var count = await query.CountAsync();
            var data = query
                .OrderBy(sortPropertyName, sortType)
                .Skip(Math.Max(pageSize * (pageNumber - 1), 0)).Take(pageSize);

            if (queryMapping)
                return new PaginationViewModel<S>
                {
                    Data = await data.ProjectTo<S>(configurationProvider).ToListAsync(),
                    Total = count
                };

            var mapper = new Mapper(configurationProvider);
            return new PaginationViewModel<S>
            {
                Data = mapper.Map<List<S>>(await data.ToListAsync()),
                Total = count
            };
        }

        public IRepositoryBuilder<T> AsNoTracking(bool tracking = false)
        {
            return new RepositoryBuilder<T>(this).AsNoTracking(tracking);
        }

        public IRepositoryBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            return new RepositoryBuilder<T>(this).Where(predicate);
        }

        public IRepositoryBuilder<T> Include(Expression<Func<T, object>> include)
        {
            return new RepositoryBuilder<T>(this).Include(include);
        }

        public IRepositoryBuilder<T> OrderBy(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return new RepositoryBuilder<T>(this).OrderBy(orderBy);
        }

        public virtual async Task<T> GetById<S>(S id,
            List<Expression<Func<T, object>>> includes = null)
        {
            var query = _dbContext.Set<T>();

            var entity = await query.FindAsync(id);

            if (includes != null)
                await Task.WhenAll(includes.Select(include =>
                    _dbContext.Entry(entity).Reference(include).LoadAsync()));

            return entity;
        }

        public T FirstOrDefault(List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = false,
            bool filterDeletedItem = true)
        {
            return Queryable(predicates, orderBys, includes, disableTracking, filterDeletedItem).FirstOrDefault();
        }

        public int Count(List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool filterDeletedItem = true)
        {
            return Queryable(predicates, orderBys, includes, true, filterDeletedItem).Count();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity;
        }

        public void Update(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            entity.IsDeleted = true;
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            foreach (var entityBase in entities)
            {
                _dbContext.Attach(entityBase);
                entityBase.IsDeleted = true;
            }
        }
    }
}