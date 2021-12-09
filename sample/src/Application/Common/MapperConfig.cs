using System;
using Application.Contracts.Persistence;
using Application.DTOs.News;
using Application.DTOs.User;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common;

public class MapperConfig : Profile
{
    public MapperConfig(IServiceProvider provider)
    {
        // News
        CreateMap<News, NewsDto>();
        CreateMap<News, NewsListItemDto>();

        // User
        CreateMap<User, ShortProfileDto>();
        CreateMap<User, ProfileDto>()
            .ForMember(des => des.Roles,
                opt =>
                    opt.MapFrom((src, des) =>
                    {
                        return provider.GetService<IMemoryCache>().GetOrCreate("UserRoles:" + src.Id, entry =>
                        {
                            entry.SlidingExpiration = TimeSpan.FromDays(7);
                            return provider.GetService<IUnitOfWork>()?.Repository<IUserRepository, User>()
                                .GetUserRoles(src.Id)
                                .Result;
                        });
                    }));
    }
}