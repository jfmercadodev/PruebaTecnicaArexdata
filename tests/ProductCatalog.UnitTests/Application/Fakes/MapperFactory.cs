using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using ProductCatalog.Application.Products.Mappings;

namespace ProductCatalog.UnitTests.Application;

internal static class MapperFactory
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(
            cfg => cfg.AddProfile<ProductProfile>(),
            NullLoggerFactory.Instance);
        return configuration.CreateMapper();
    }
}
