using System;
using System.Linq;
using Domain.Common;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Application.Common.Models;
using System.Collections.Generic;
using Application.Common.Enums;
using AutoMapper;

namespace Application.Contracts.Persistence
{
    public interface IRepositoryBuilder<T> where T : IEntityBase
    {
        T FirstOrDefault();
        Task<T> GetById<S>(S id);
        IQueryable<T> Queryable();
        Task<IReadOnlyList<T>> Get();
        IRepositoryBuilder<T> AsNoTracking(bool tracking = false);
        IRepositoryBuilder<T> Where(Expression<Func<T, bool>> predicate);
        IRepositoryBuilder<T> Include(Expression<Func<T, object>> include);
        Task<PaginationViewModel<T>> GetPage(int pageNumber, int pageSize,
            string sortPropertyName = "", SortType? sortType = SortType.Ascending);
        Task<PaginationViewModel<S>> GetPage<S>(IConfigurationProvider configurationProvider, int pageNumber, int pageSize,
            string sortPropertyName = "", SortType? sortType = SortType.Ascending, bool queryMapping = true);
        IRepositoryBuilder<T> OrderBy(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy);
    }
}