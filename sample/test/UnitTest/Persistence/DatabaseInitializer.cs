using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTest.Persistence
{
    public class DatabaseInitializer
    {
        private DatabaseContext Context { get; }
        private IConfiguration Configuration { get; }
        private UserManager<User> UserManager { get; }
        private RoleManager<IdentityRole> RoleManager { get; }

        public DatabaseInitializer(IServiceProvider scopeServiceProvider)
        {
            Context = scopeServiceProvider.GetService<DatabaseContext>();
            Configuration = scopeServiceProvider.GetService<IConfiguration>();
            UserManager = scopeServiceProvider.GetService<UserManager<User>>();
            RoleManager = scopeServiceProvider.GetService<RoleManager<IdentityRole>>();
        }

        public async Task Initialize()
        {
            try
            {
                if (await Context.Database.EnsureCreatedAsync())
                    await Initializer();
                else
                    while (!Context.InitializeHistories.Any(history => history.Version == "V1"))
                        Thread.Sleep(500);
            }
            catch (Exception e)
            {
                Debugger.Break();
                // LoggerService.Error(e);
            }
        }

        private async Task Initializer()
        {
            const string version = "V1";

            if (Context.InitializeHistories.Any(history => history.Version == version))
                return;

            await RoleInitializer();
            await UserInitializer();
            await NewsInitializer();

            await Context.InitializeHistories.AddAsync(new InitializeHistory
            {
                Version = version
            });

            await Context.SaveChangesAsync();
        }

        private async Task RoleInitializer()
        {
            await RoleManager.CreateAsync(new IdentityRole {Name = "Owner"});
            await RoleManager.CreateAsync(new IdentityRole {Name = "Admin"});
            await RoleManager.CreateAsync(new IdentityRole {Name = "User"});
        }

        private async Task UserInitializer()
        {
            var owner = new User
            {
                Id = "OwnerUser",
                UserName = "OwnerUser",
                IsEnable = true,
                LastName = "Owner",
                FirstName = "Owner",
                PhoneNumberConfirmed = true,
                PhoneNumber = "0098 12345679",
                Email = "TestOwner@Markop.com",
            };
            await UserManager.CreateAsync(owner, "OwnerPassword");
            await UserManager.AddToRoleAsync(owner, "Owner");
            var ownerToken1 = await UserManager.GenerateEmailConfirmationTokenAsync(owner);
            await UserManager.ConfirmEmailAsync(owner, ownerToken1);

            var userTest1 = new User
            {
                Id = "user1",
                UserName = "user1",
                IsEnable = true,
                LastName = "Test",
                FirstName = "User1",
                PhoneNumberConfirmed = true,
                PhoneNumber = "0098 12345680",
                Email = "TestUser@Markop.com",
            };
            await UserManager.CreateAsync(userTest1, "TestPassword");
            await UserManager.AddToRoleAsync(userTest1, "User");
            var userToken1 = await UserManager.GenerateEmailConfirmationTokenAsync(userTest1);
            await UserManager.ConfirmEmailAsync(userTest1, userToken1);

            var userTest2 = new User
            {
                Id = "user2",
                UserName = "user2",
                IsEnable = true,
                LastName = "Test",
                FirstName = "User2",
                PhoneNumberConfirmed = true,
                PhoneNumber = "0098 12345681",
                Email = "TestUser2@Markop.com",
            };
            await UserManager.CreateAsync(userTest2, "TestPassword");
            await UserManager.AddToRoleAsync(userTest2, "User");
            var userToken2 = await UserManager.GenerateEmailConfirmationTokenAsync(userTest2);
            await UserManager.ConfirmEmailAsync(userTest1, userToken2);

            var userTest3 = new User
            {
                Id = "user3",
                UserName = "user3",
                IsEnable = true,
                LastName = "Test",
                FirstName = "User3",
                PhoneNumberConfirmed = true,
                PhoneNumber = "0098 12345682",
                Email = "TestUser4@Markop.com",
            };
            await UserManager.CreateAsync(userTest3, "TestPassword");
            await UserManager.AddToRoleAsync(userTest3, "User");

            var userTest4 = new User
            {
                Id = "user4",
                UserName = "user4",
                IsEnable = true,
                LastName = "Test",
                FirstName = "User4",
                PhoneNumberConfirmed = true,
                PhoneNumber = "0098 12346798",
                Email = "UnconfirmedEmail@Markop.com",
            };
            await UserManager.CreateAsync(userTest4, "TestPassword");
            await UserManager.AddToRoleAsync(userTest4, "User");
        }

        private async Task NewsInitializer()
        {
            Context.Newses.Add(new News
            {
                Title = "Title",
                IsHidden = false,
                Content = "Test Content",
                Preview = "Preview Content",
                AuthorId = Context.Users.First().Id
            });
            Context.Newses.Add(new News
            {
                Title = "Title",
                IsHidden = false,
                Content = "New Test Content",
                Preview = "New Preview Content",
                AuthorId = Context.Users.First().Id
            });

            await Context.SaveChangesAsync();
        }
    }
}