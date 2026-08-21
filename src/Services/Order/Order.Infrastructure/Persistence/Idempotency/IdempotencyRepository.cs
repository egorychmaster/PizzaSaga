using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Persistence.Idempotency;

namespace Order.Infrastructure.Persistence.Idempotency;

/// <summary>
/// EF Core реализация IIdempotencyRepository.
/// Все операции выполняются в рамках текущей транзакции DbContext.
/// </summary>
internal sealed class IdempotencyRepository : IIdempotencyRepository
{
    private readonly OrderDbContext _context;

    public IdempotencyRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IdempotencyRecordDto?> GetAsync(Guid idempotencyKey, CancellationToken cancellationToken = default)
    {
        var record = await _context.IdempotencyRecords
            .Where(x => x.IdempotencyKey == idempotencyKey)
            .Select(x => new IdempotencyRecordDto(
                IdempotencyKey: x.IdempotencyKey,
                RequestHash: x.RequestHash,
                ResponseStatusCode: x.ResponseStatusCode,
                ResponseBody: x.ResponseBody,
                CreatedAt: x.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return record;
    }

    /// <inheritdoc />
    public async Task AddAsync(IdempotencyRecordDto entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var model = new IdempotencyRecord
        {
            IdempotencyKey = entity.IdempotencyKey,
            RequestHash = entity.RequestHash,
            ResponseStatusCode = entity.ResponseStatusCode,
            ResponseBody = entity.ResponseBody,
            CreatedAt = entity.CreatedAt
        };

        // EF Core добавит сущность в ChangeTracker и выполнит INSERT при SaveChangesAsync
        await _context.IdempotencyRecords.AddAsync(model, cancellationToken);
    }
}