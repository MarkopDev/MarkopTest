using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Contracts.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Utilities;

public static class Extentions
{
    public static bool IsEmail(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return new Regex(
            @"^[A-Za-z0-9!'#$%&*+\/=?^_`{|}~-]+(?:\.[A-Za-z0-9!'#$%&*+\/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z]{2,}$",
            RegexOptions.IgnoreCase).IsMatch(text);
    }

    public static bool IsPhoneNumber(this string text)
    {
        return !string.IsNullOrWhiteSpace(text) &&
               new Regex("00(\\d{1,2})\\s([1-9]\\d{1,1})([0-9]\\d{4,})").IsMatch(text);
    }

    public static PaginationViewModel<D> To<S, D>(this PaginationViewModel<S> paginationViewModel,
        Func<S, D> convertor)
    {
        return new()
        {
            Data = paginationViewModel.Data.Select(convertor).ToList(),
            Total = paginationViewModel.Total
        };
    }

    public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string properties,
        SortType? sortType = SortType.Ascending)
    {
        if (string.IsNullOrEmpty(properties))
            return query;

        var orderByMethod = sortType == SortType.Descending ? "OrderByDescending" : "OrderBy";

        var propertiesName = properties.Split(".");

        var parameterExpression = Expression.Parameter(query.ElementType);

        MemberExpression memberExpression = null;
        foreach (var propertyName in propertiesName)
            memberExpression = Expression.Property((Expression) memberExpression ?? parameterExpression,
                propertyName);

        var orderByCall = Expression.Call(typeof(Queryable),
            orderByMethod,
            new[] {query.ElementType, memberExpression?.Type},
            query.Expression,
            Expression.Quote(Expression.Lambda(memberExpression!, parameterExpression)));

        return query.Provider.CreateQuery(orderByCall) as IQueryable<T>;
    }

    public static string GetUserId(this IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static User GetUser(this IHttpContextAccessor httpContextAccessor, bool asNoTracking = true)
    {
        var unitOfWork = (IUnitOfWork) httpContextAccessor.HttpContext?.RequestServices.GetService(typeof(IUnitOfWork));
        var memoryCache = (IMemoryCache) httpContextAccessor.HttpContext?.RequestServices.GetService(typeof(IMemoryCache));
            
        var userId = httpContextAccessor.GetUserId();
            
        var user = memoryCache.GetOrCreate("User:" + userId, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
            return unitOfWork?.Repository<User>()
                .AsNoTracking(asNoTracking)
                .Where(u => u.Id == userId).FirstOrDefault();
        });

        if (user == null)
            throw new ServiceException(new Error
            {
                Code = ErrorCode.Unauthorized,
                Message = "Unauthorized user."
            });

        return user;
    }
}