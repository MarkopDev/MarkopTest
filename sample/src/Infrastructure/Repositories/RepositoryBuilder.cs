using System;
using System.Linq;
using Domain.Common;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Application.Common.Enums;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;

namespace Infrastructure.Repositories
{
    public class RepositoryBuilder<T> : IRepositoryBuilder<T> where T : IEntityBase
    {
        private IRepository<T> Repository { get; }
        private bool? DisableTracking { get; set; }
        private bool FilterDeletedItem { get; set; } = true;
        private List<Expression<Func<T, bool>>> Predicates { get; } = new();
        private List<Expression<Func<T, object>>> Includes { get; } = new();
        private List<Func<IQueryable<T>, IOrderedQueryable<T>>> OrderBys { get; } = new();

        public RepositoryBuilder(IRepository<T> repository)
        {
            Repository = repository;
        }

        public T? FirstOrDefault()
        {
            if (DisableTracking.HasValue)
                return Repository.FirstOrDefault(Predicates, OrderBys, Includes, DisableTracking.Value,
                    FilterDeletedItem);
            return Repository.FirstOrDefault(Predicates, OrderBys, Includes, filterDeletedItem: FilterDeletedItem);
        }

        public Task<T> GetById<S>(S id)
        {
            return Repository.GetById(id, Includes);
        }

        public Task<IReadOnlyList<T>> Get()
        {
            if (DisableTracking.HasValue)
                return Repository.Get(Predicates, OrderBys, Includes, DisableTracking.Value, FilterDeletedItem);
            return Repository.Get(Predicates, OrderBys, Includes, filterDeletedItem: FilterDeletedItem);
        }

        public IQueryable<T> Queryable()
        {
            if (DisableTracking.HasValue)
                return Repository.Queryable(Predicates, OrderBys, Includes, DisableTracking.Value, FilterDeletedItem);
            return Repository.Queryable(Predicates, OrderBys, Includes, filterDeletedItem: FilterDeletedItem);
        }

        public Task<PaginationViewModel<T>> GetPage(int pageNumber, int pageSize,
            string sortPropertyName = "", SortType? sortType = SortType.Ascending)
        {
            if (DisableTracking.HasValue)
                return Repository.GetPage(pageNumber, pageSize, sortPropertyName, sortType, Predicates, OrderBys,
                    Includes, DisableTracking.Value, FilterDeletedItem);
            return Repository.GetPage(pageNumber, pageSize, sortPropertyName, sortType, Predicates, OrderBys, Includes,
                filterDeletedItem: FilterDeletedItem);
        }

        public Task<PaginationViewModel<S>> GetPage<S>(IConfigurationProvider configurationProvider, int pageNumber,
            int pageSize, string sortPropertyName = "", SortType? sortType = SortType.Ascending, bool queryMapping = true)
        {
            if (DisableTracking.HasValue)
                return Repository.GetPage<S>(configurationProvider, pageNumber, pageSize, sortPropertyName, sortType,
                    Predicates, OrderBys, Includes, DisableTracking.Value, FilterDeletedItem, queryMapping);
            return Repository.GetPage<S>(configurationProvider, pageNumber, pageSize, sortPropertyName, sortType,
                Predicates, OrderBys, Includes, filterDeletedItem: FilterDeletedItem, queryMapping: queryMapping);
        }

        public IRepositoryBuilder<T> AsNoTracking(bool tracking = false)
        {
            DisableTracking = !tracking;
            return this;
        }

        public IRepositoryBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            Predicates.Add(predicate);
            return this;
        }

        public IRepositoryBuilder<T> Include(Expression<Func<T, object>> include)
        {
            Includes.Add(include);
            return this;
        }

        public IRepositoryBuilder<T> OrderBy(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            OrderBys.Add(orderBy);
            return this;
        }
    }
}