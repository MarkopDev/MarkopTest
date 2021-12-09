using System;
using System.Linq;
using Domain.Entities;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

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
            // await Context.Database.MigrateAsync();

            if (!await Context.Database.EnsureCreatedAsync())
                return;

            await InitializerV1();
        }
        catch (Exception)
        {
            // LoggerService.Error(e);
            Debugger.Break();
        }
    }

    private async Task InitializerV1()
    {
        const string version = "V1";

        if (Context.InitializeHistories.Any(history => history.Version == version))
            return;

        await RoleInitializer();
        await OwnerInitializer();

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

    private async Task OwnerInitializer()
    {
        var owner = new User
        {
            LastName = "System",
            FirstName = "Owner",
            Email = "Owner@Markop.com",
            PhoneNumberConfirmed = true,
            PhoneNumber = "001 12345678",
        };
        await UserManager.CreateAsync(owner, "OwnerPassword");
        await UserManager.AddToRoleAsync(owner, "Owner");
        var ownerToken1 = await UserManager.GenerateEmailConfirmationTokenAsync(owner);
        await UserManager.ConfirmEmailAsync(owner, ownerToken1);
    }
}