using System.Threading;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Contracts.Persistence;
using Application.DTOs.User;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Account.Commands.SignUp
{
    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, SignUpViewModel>
    {
        private IMapper Mapper { get; }
        private IUnitOfWork UnitOfWork { get; }
        private UserManager<Domain.Entities.User> UserManager { get; }
        private SignInManager<Domain.Entities.User> SignInManager { get; }

        public SignUpCommandHandler(UserManager<Domain.Entities.User> userManager, 
            SignInManager<Domain.Entities.User> signInManager, IMapper mapper, IUnitOfWork unitOfWork)
        {
            Mapper = mapper;
            UnitOfWork = unitOfWork;
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public async Task<SignUpViewModel> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            var duplicateUser = UnitOfWork.Repository<Domain.Entities.User>()
                .Where(u => u.PhoneNumber == request.PhoneNumber).FirstOrDefault();
            if (duplicateUser != null)
            {
                if (duplicateUser.PhoneNumberConfirmed)
                    throw new ServiceException(new Error
                    {
                        Code = ErrorCode.Duplicate,
                        Message = "Duplicate phone number."
                    });
                await UserManager.DeleteAsync(duplicateUser);
            }

            var user = new Domain.Entities.User
            {
                LastName = request.LastName,
                FirstName = request.FirstName,
                PhoneNumber = request.PhoneNumber,
                UserName = request.PhoneNumber.Replace(" ", ""),
            };

            var result = await UserManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                throw new ServiceException(new Error
                {
                    Code = ErrorCode.Unexpected,
                    Message = "Unexpected occurred while sign up your account."
                });

            await UserManager.AddToRoleAsync(user, "User");

            return new SignUpViewModel
            {
                Profile = Mapper.Map<ProfileDto>(user)
            };
        }
    }
}