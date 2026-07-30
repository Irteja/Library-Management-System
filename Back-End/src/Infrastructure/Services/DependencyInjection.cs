using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Infrastructure.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}