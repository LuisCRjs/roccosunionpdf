using System.Data;
using System.Data.Common;
using DocumentManager.Core.Models;
using DocumentManager.Core.Services;
using DocumentManager.Core.Services.Interfaces;
using DocumentManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DocumentManager.Infrastructure.Services;

public sealed class RecordService(IDbContextFactory<AppDbContext> contextFactory) : IRecordService
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "INSERT OR IGNORE INTO FolioSequences (Id, LastValue) VALUES (1, 0);",
            cancellationToken);
    }

    public async Task<string> ReserveNextInternalFolioAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandText = "UPDATE FolioSequences SET LastValue = LastValue + 1 WHERE Id = 1 RETURNING LastValue;";

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        if (scalar is null || scalar is DBNull)
        {
            throw new InvalidOperationException("No fue posible reservar el siguiente folio interno.");
        }

        var sequence = Convert.ToInt64(scalar, System.Globalization.CultureInfo.InvariantCulture);
        await transaction.CommitAsync(cancellationToken);
        return FolioFormatter.Format(sequence);
    }

    public async Task<ServiceRecord> CreateAsync(
        ServiceRecord record,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.ServiceRecords.Add(record);
        await context.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task<IReadOnlyList<ServiceRecord>> SearchAsync(
        string? searchText,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.ServiceRecords.AsNoTracking();
        var normalized = searchText?.Trim();

        if (!string.IsNullOrEmpty(normalized))
        {
            var normalizedUpper = normalized.ToUpperInvariant();
            query = query.Where(record =>
                record.InternalFolio.ToUpper().Contains(normalizedUpper) ||
                record.ServiceOrderFolio.ToUpper().Contains(normalizedUpper));
        }

        return await query
            .OrderByDescending(record => record.Date)
            .ThenByDescending(record => record.Id)
            .ToListAsync(cancellationToken);
    }
}
