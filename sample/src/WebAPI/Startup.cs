using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Identity;
using Infrastructure.MiddleWare;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebAPI;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddControllersWithViews();

        services.AddApplicationServices(Configuration);
        services.AddInfrastructureServices(Configuration);
            
        #region Authorization

        services.AddIdentity<User, IdentityRole>()
            .AddUserManager<UserManager<User>>()
            .AddEntityFrameworkStores<DatabaseContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IAuthorizationHandler, AuthorizationHandler>();

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 8;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "token";
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
        });

        services.AddAuthorization(option =>
        {
            option.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddRequirements(new AuthorizationRequirements(new List<string> {"User", "Admin", "Owner"}))
                .Build();
            option.AddPolicy("OwnerPolicy", policy =>
                policy.AddRequirements(new AuthorizationRequirements(new List<string> {"Owner"})));
            option.AddPolicy("AdminOwnerPolicy", policy =>
                policy.AddRequirements(new AuthorizationRequirements(new List<string> {"Admin", "Owner"})));
            option.AddPolicy("UserPolicy", policy =>
                policy.AddRequirements(new AuthorizationRequirements(new List<string> {"User"})));
        });

        #endregion
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();

        app.UseRouting();
            
        app.UseAuthentication();
        app.UseAuthorization();
            
        app.UseMiddleware<SystemExceptionMiddleWare>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");
        });
    }
}