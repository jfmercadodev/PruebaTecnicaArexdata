using System.Reflection;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ProductCatalog.Application.Common.Behaviors;

namespace ProductCatalog.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        RegisterValidators(services, assembly);
        RegisterMapper(services, assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        return services;
    }

    private static void RegisterValidators(IServiceCollection services, Assembly assembly)
    {
        var validatorRegistrations = assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition)
            .Select(type => new
            {
                ImplementationType = type,
                ServiceTypes = type
                    .GetInterfaces()
                    .Where(candidate =>
                        candidate.IsGenericType &&
                        candidate.GetGenericTypeDefinition() == typeof(IValidator<>))
                    .ToArray()
            })
            .Where(registration => registration.ServiceTypes.Length > 0);

        foreach (var registration in validatorRegistrations)
        {
            foreach (var serviceType in registration.ServiceTypes)
            {
                services.AddTransient(serviceType, registration.ImplementationType);
            }
        }
    }

    private static void RegisterMapper(IServiceCollection services, Assembly assembly)
    {
        var mapperConfiguration = new MapperConfiguration(
            configuration => configuration.AddMaps(assembly),
            NullLoggerFactory.Instance);

        services.AddSingleton<IConfigurationProvider>(mapperConfiguration);
        services.AddSingleton<IMapper>(provider =>
            new Mapper(provider.GetRequiredService<IConfigurationProvider>(), provider.GetService));
    }
}
