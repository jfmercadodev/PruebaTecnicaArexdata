using ProductCatalog.Domain.Common;
using ProductCatalog.Domain.Events;
using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Domain.Entities;

public sealed class Product : AggregateRoot<Guid>
{
    private Product(Guid id, string name, Sku sku, Money salePrice, Money cost, int stock)
        : base(id)
    {
        Name = name;
        Sku = sku;
        SalePrice = salePrice;
        Cost = cost;
        Stock = stock;
    }

    public string Name { get; private set; }

    public Sku Sku { get; private set; }

    public Money SalePrice { get; private set; }

    public Money Cost { get; private set; }

    public int Stock { get; private set; }

    public decimal MarginPercent =>
        SalePrice.Value == 0
            ? 0
            : decimal.Round((SalePrice.Value - Cost.Value) / SalePrice.Value * 100m, 4, MidpointRounding.AwayFromZero);

    public static Product Create(
        string name,
        Sku sku,
        Money salePrice,
        Money cost,
        int stock,
        Guid? id = null)
    {
        ValidateName(name);
        ValidatePrices(salePrice, cost);
        ValidateStock(stock);

        var product = new Product(id ?? Guid.NewGuid(), name.Trim(), sku, salePrice, cost, stock);
        product.RegisterCreatedEvent();
        return product;
    }

    public void UpdatePrice(decimal salePrice, decimal cost)
    {
        var newSalePrice = Money.Create(salePrice);
        var newCost = Money.Create(cost);

        ValidatePrices(newSalePrice, newCost);

        SalePrice = newSalePrice;
        Cost = newCost;
        RegisterUpdatedEvent();
    }

    public void Rename(string name)
    {
        ValidateName(name);

        var trimmedName = name.Trim();
        if (Name == trimmedName)
        {
            return;
        }

        Name = trimmedName;
        RegisterUpdatedEvent();
    }

    public void ChangeSku(Sku sku)
    {
        ArgumentNullException.ThrowIfNull(sku);

        if (Sku == sku)
        {
            return;
        }

        Sku = sku;
        RegisterUpdatedEvent();
    }

    public void AdjustStock(int delta)
    {
        var newStock = Stock + delta;
        ValidateStock(newStock);

        Stock = newStock;
        RegisterUpdatedEvent();
    }

    public void ExecuteAtomic(Action<Product> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);

        var snapshot = new ProductSnapshot(Name, Sku, SalePrice, Cost, Stock, DomainEvents.ToArray());

        try
        {
            changes(this);
        }
        catch
        {
            RestoreSnapshot(snapshot);
            throw;
        }
    }

    private static void ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidProductNameException("El nombre del producto no puede estar vacio.");
        }

        var trimmed = name.Trim();
        if (trimmed.Length is < 3 or > 200)
        {
            throw new InvalidProductNameException("El nombre del producto debe tener entre 3 y 200 caracteres.");
        }
    }

    private static void ValidatePrices(Money salePrice, Money cost)
    {
        if (salePrice < cost)
        {
            throw new InvalidPriceException(salePrice.Value, cost.Value);
        }
    }

    private static void ValidateStock(int stock)
    {
        if (stock < 0)
        {
            throw new InvalidStockException(stock);
        }
    }

    private void RegisterCreatedEvent()
    {
        AddDomainEvent(
            new ProductCreated(
                Id,
                Sku.Value,
                Name,
                SalePrice.Value,
                Cost.Value,
                Stock,
                DateTime.UtcNow));
    }

    private void RegisterUpdatedEvent()
    {
        if (DomainEvents.OfType<ProductCreated>().Any())
        {
            return;
        }

        var newEvents = DomainEvents
            .Where(domainEvent => domainEvent is not ProductUpdated)
            .ToList();

        newEvents.Add(
            new ProductUpdated(
                Id,
                Sku.Value,
                SalePrice.Value,
                Cost.Value,
                Stock,
                DateTime.UtcNow));

        ReplaceDomainEvents(newEvents);
    }

    private void RestoreSnapshot(ProductSnapshot snapshot)
    {
        Name = snapshot.Name;
        Sku = snapshot.Sku;
        SalePrice = snapshot.SalePrice;
        Cost = snapshot.Cost;
        Stock = snapshot.Stock;
        ReplaceDomainEvents(snapshot.DomainEvents);
    }

    private sealed record ProductSnapshot(
        string Name,
        Sku Sku,
        Money SalePrice,
        Money Cost,
        int Stock,
        IReadOnlyCollection<DomainEvent> DomainEvents);
}
