using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Common.Models;
using ProductCatalog.Infrastructure.Persistence;
using ProductCatalog.Infrastructure.Persistence.Entities;

namespace ProductCatalog.Infrastructure.Idempotency;

public sealed class EfIdempotencyStore : IIdempotencyStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly AppDbContext _dbContext;

    public EfIdempotencyStore(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IdempotencyRecord<TResponse>?> GetAsync<TResponse>(Guid requestId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProcessedRequests
            .AsNoTracking()
            .SingleOrDefaultAsync(record => record.RequestId == requestId, cancellationToken);

        if (entity is null || entity.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return null;
        }

        var response = JsonSerializer.Deserialize<TResponse>(entity.PayloadJson, SerializerOptions);
        if (response is null)
        {
            return null;
        }

        return new IdempotencyRecord<TResponse>(requestId, response);
    }

    public async Task SaveAsync<TResponse>(
        Guid requestId,
        TResponse response,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProcessedRequests.SingleOrDefaultAsync(
            record => record.RequestId == requestId,
            cancellationToken);

        if (entity is null)
        {
            entity = new ProcessedRequestEntity
            {
                RequestId = requestId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.ProcessedRequests.Add(entity);
        }

        entity.ResponseType = typeof(TResponse).AssemblyQualifiedName ?? typeof(TResponse).FullName ?? typeof(TResponse).Name;
        entity.PayloadJson = JsonSerializer.Serialize(response, SerializerOptions);
        entity.ExpiresAtUtc = DateTime.UtcNow.Add(window);
    }
}
