using MediatR;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using Application.Contracts.Persistence;
using Application.DTOs.User;
using Application.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.User.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, GetProfileViewModel>
{
    private IMapper Mapper { get; }
    private IUnitOfWork UnitOfWork { get; }
    private IHttpContextAccessor HttpContextAccessor { get; }
    private UserManager<Domain.Entities.User> UserManager { get; }

    public GetProfileQueryHandler(UserManager<Domain.Entities.User> userManager, IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor, IMapper mapper)
    {
        Mapper = mapper;
        UnitOfWork = unitOfWork;
        UserManager = userManager;
        HttpContextAccessor = httpContextAccessor;
    }

    public Task<GetProfileViewModel> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = HttpContextAccessor.GetUser();

        return Task.FromResult(new GetProfileViewModel
        {
            Profile = Mapper.Map<ProfileDto>(user)
        });
    }
}