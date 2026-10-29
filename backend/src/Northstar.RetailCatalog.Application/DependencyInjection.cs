using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Northstar.RetailCatalog.Application.Abstractions;
using Northstar.RetailCatalog.Application.Common.Behaviors;

namespace Northstar.RetailCatalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
