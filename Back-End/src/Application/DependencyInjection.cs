using System.Reflection;
using FluentValidation;
using LibraryManagementSystem.Application.Common.Behaviours;
using LibraryManagementSystem.Application.Common.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IMemberAccessService, MemberAccessService>();

        return services;
    }
}