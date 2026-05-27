using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.ValueObjects;
using ProductCatalog.Infrastructure.Persistence;

namespace ProductCatalog.Infrastructure.Seeding;

public sealed class ProductCatalogSeeder
{
    public async Task<int> SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            return 0;
        }

        var products = GetSeedProducts();
        dbContext.Products.AddRange(products);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyCollection<Product> GetSeedProducts()
    {
        return new[]
        {
            CreateSeedProduct("Mechanical Keyboard", "KB-001", 199.90m, 129.50m, 14),
            CreateSeedProduct("Wireless Mouse", "MS-002", 89.90m, 42.20m, 35),
            CreateSeedProduct("27 Inch Monitor", "MN-003", 1199.00m, 840.00m, 8),
            CreateSeedProduct("USB-C Dock", "DK-004", 349.99m, 210.00m, 19),
            CreateSeedProduct("Laptop Stand", "LS-005", 129.00m, 58.40m, 27),
            CreateSeedProduct("Noise Cancelling Headphones", "HP-006", 899.50m, 620.10m, 11),
            CreateSeedProduct("Webcam 4K", "WC-007", 415.75m, 250.00m, 9),
            CreateSeedProduct("Portable SSD 1TB", "SD-008", 529.95m, 401.35m, 22),
            CreateSeedProduct("Office Chair", "CH-009", 1499.00m, 1099.00m, 5),
            CreateSeedProduct("Desk Lamp", "LP-010", 74.80m, 29.99m, 41),
            CreateSeedProduct("Graphic Tablet", "GT-011", 655.00m, 440.00m, 7),
            CreateSeedProduct("Microphone USB", "MC-012", 278.60m, 170.00m, 16),
            CreateSeedProduct("Router WiFi 6", "RT-013", 489.30m, 312.45m, 13),
            CreateSeedProduct("Smart Speaker", "SP-014", 210.00m, 132.10m, 24),
            CreateSeedProduct("Streaming Light", "SL-015", 99.95m, 48.00m, 29),
            CreateSeedProduct("Ergonomic Desk", "DS-016", 2299.00m, 1750.00m, 4),
            CreateSeedProduct("Gaming Controller", "GC-017", 259.99m, 144.35m, 18),
            CreateSeedProduct("Projector Mini", "PJ-018", 980.00m, 700.00m, 6),
            CreateSeedProduct("Mesh Network Node", "MN-019", 365.25m, 210.00m, 12),
            CreateSeedProduct("Bluetooth Tracker", "BT-020", 45.90m, 15.75m, 48)
        };
    }

    private static Product CreateSeedProduct(
        string name,
        string sku,
        decimal salePrice,
        decimal cost,
        int stock)
    {
        var product = Product.Create(
            name,
            Sku.Create(sku),
            Money.Create(salePrice),
            Money.Create(cost),
            stock);

        product.ClearDomainEvents();
        return product;
    }
}
