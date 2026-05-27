using ProductCatalog.Web.Components;
using ProductCatalog.Web.Middleware;
using ProductCatalog.Web.Services;
using ProductCatalog.Application.DependencyInjection;
using ProductCatalog.Infrastructure.DependencyInjection;
using ProductCatalog.Infrastructure.Monitoring;
using ProductCatalog.Infrastructure.Startup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using Serilog;
using System.Globalization;

namespace ProductCatalog.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Host.UseSerilog((context, services, configuration) =>
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "ProductCatalog.Web"));

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("La cadena de conexion 'DefaultConnection' es obligatoria.");
        }

        // Add services to the container.
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(
            connectionString,
            options => builder.Configuration
                .GetSection(QueryInspectionOptions.SectionName)
                .Bind(options));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<CatalogProblemDetailsFactory>();
        builder.Services.AddScoped<ProductCatalogFacade>();
        builder.Services.AddTransient<CorrelationIdMiddleware>();
        builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validacion fallida",
                        Detail = "Revisa los datos enviados y corrige los campos resaltados.",
                        Type = "https://httpstatuses.com/400",
                        Instance = context.HttpContext.Request.Path
                    };

                    problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                    problemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;

                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var supportedCultures = new[] { new CultureInfo("es-CO") };
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("es-CO");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.ApplyCurrentCultureToResponseHeaders = true;
        });

        var app = builder.Build();
        await app.Services.InitializeCatalogDatabaseAsync();

        // Configure the HTTP request pipeline.
        app.UseRequestLocalization();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapControllers();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        await app.RunAsync();
    }
}
