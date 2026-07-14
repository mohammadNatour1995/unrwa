using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;

namespace Application;

public static class InfrastructureInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped(typeof(IEFBaseRepository<>), typeof(EFBaseRepository<>));
        services.AddScoped(typeof(IEFBaseLookupRepository<>), typeof(EFBaseLookupRepository<>));
        services.AddScoped(typeof(IDapperRepository<>), typeof(DapperRepository<>));

        return services;
    }
}
