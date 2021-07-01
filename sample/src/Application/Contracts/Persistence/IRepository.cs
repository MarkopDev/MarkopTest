using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Models;
using AutoMapper;
using Domain.Common;

namespace Application.Contracts.Persistence
{
    public interface IRepository<T> where T : IEntityBase
    {
        IQueryable<T> Queryable(List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = true,
            bool filterDeletedItem = true);

        Task<IReadOnlyList<T>> Get(List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = true,
            bool filterDeletedItem = true);

        Task<PaginationViewModel<T>> GetPage(int pageNumber, int pageSize,
            string sortPropertyName = "", SortType? sortType = SortType.Ascending,
            List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = true,
            bool filterDeletedItem = true);

        Task<PaginationViewModel<S>> GetPage<S>(IConfigurationProvider configurationProvider, int pageNumber,
            int pageSize, string sortPropertyName = "", SortType? sortType = SortType.Ascending,
            List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = true,
            bool filterDeletedItem = true,
            bool queryMapping = true);

        IRepositoryBuilder<T> AsNoTracking(bool tracking = false);
        IRepositoryBuilder<T> Where(Expression<Func<T, bool>> predicate);
        IRepositoryBuilder<T> Include(Expression<Func<T, object>> include);
        IRepositoryBuilder<T> OrderBy(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy);

        Task<T> GetById<S>(S id, List<Expression<Func<T, object>>> includes = null);

        T? FirstOrDefault(List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = false,
            bool filterDeletedItem = true);

        int Count(List<Expression<Func<T, bool>>> predicates = null,
            List<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBys = null,
            List<Expression<Func<T, object>>> includes = null,
            bool filterDeletedItem = true);

        Task<T> AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
    }
}