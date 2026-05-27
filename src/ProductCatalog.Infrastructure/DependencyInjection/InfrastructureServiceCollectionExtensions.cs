using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.Interfaces;
using ProductCatalog.Infrastructure.Caching;
using ProductCatalog.Infrastructure.Events;
using ProductCatalog.Infrastructure.Idempotency;
using ProductCatalog.Infrastructure.Monitoring;
using ProductCatalog.Infrastructure.Persistence;
using ProductCatalog.Infrastructure.Repositories;
using ProductCatalog.Infrastructure.Seeding;

namespace ProductCatalog.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        Action<QueryInspectionOptions>? configureMonitoring = null)
    {
        services.AddMemoryCache();
        services.AddScoped<QueryInspectionState>();
        services.AddScoped<QueryInspectionInterceptor>();
        services.AddScoped<ProductCatalogSeeder>();

        services.AddOptions<QueryInspectionOptions>();
        if (configureMonitoring is not null)
        {
            services.Configure(configureMonitoring);
        }

        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            options.UseSqlServer(connectionString);
            options.AddInterceptors(provider.GetRequiredService<QueryInspectionInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
        services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();
        services.AddScoped<IDomainEventPublisher, MediatRDomainEventPublisher>();

        services.AddSingleton<MemoryQueryCache>();
        services.AddSingleton<IQueryCache>(provider => provider.GetRequiredService<MemoryQueryCache>());
        services.AddSingleton<ICacheInvalidationService>(provider => provider.GetRequiredService<MemoryQueryCache>());

        return services;
    }
}
