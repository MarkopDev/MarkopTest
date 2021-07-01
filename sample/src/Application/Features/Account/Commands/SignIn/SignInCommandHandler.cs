using System.Threading;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Contracts.Persistence;
using Application.DTOs.User;
using Application.Utilities;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Account.Commands.SignIn
{
    public class SignInCommandHandler : IRequestHandler<SignInCommand, SignInViewModel>
    {
        private IMapper Mapper { get; }
        private IUnitOfWork UnitOfWork { get; }
        private UserManager<Domain.Entities.User> UserManager { get; }
        private SignInManager<Domain.Entities.User> SignInManager { get; }

        public SignInCommandHandler(UserManager<Domain.Entities.User> userManager,
            SignInManager<Domain.Entities.User> signInManager, IMapper mapper, IUnitOfWork unitOfWork)
        {
            Mapper = mapper;
            UnitOfWork = unitOfWork;
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public async Task<SignInViewModel> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            var user = request.Type switch
            {
                LoginType.Email => await UserManager.FindByEmailAsync(request.Login.EmailNormalize()),
                LoginType.PhoneNumber => UnitOfWork.Repository<Domain.Entities.User>()
                    .Where(u => u.PhoneNumber == request.Login)
                    .FirstOrDefault(),
                _ => throw new ServiceException(new Error
                {
                    Code = ErrorCode.InvalidInput,
                    Message = "Invalid login type."
                })
            };
            
            if (user == null)
                throw new ServiceException(new Error
                {
                    Code = ErrorCode.InvalidInput,
                    Message = "Invalid login information."
                });

            var result = await SignInManager.PasswordSignInAsync(user, request.Password, true, false);

            if (!result.Succeeded)
                throw new ServiceException(new Error
                {
                    Code = ErrorCode.InvalidInput,
                    Message = "Invalid login information."
                });

            return new SignInViewModel
            {
                Profile = Mapper.Map<ProfileDto>(user)
            };
        }
    }
}