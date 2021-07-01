using System;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Application.Contracts.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DatabaseContext>(option =>
            {
                var stringConnection = Environment.GetEnvironmentVariable("DBConnectionPostgreSQL");
                option.UseNpgsql(stringConnection ?? configuration.GetConnectionString("DBConnectionPostgreSQL"));
            }, ServiceLifetime.Transient);

            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient(typeof(IRepository<>), typeof(RepositoryBase<>));
        }
    }
}