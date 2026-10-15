using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.API.Authorization;

public static class RoleAdministrationLock
{
    public static async Task<IDbContextTransaction> AcquireAsync(
        ApplicationDbContext database,
        CancellationToken cancellationToken)
    {
        var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            // All API role/permission writers must use this same transaction-level lock.
            // PostgreSQL releases it automatically on commit or rollback.
            await database.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(1144212813, 1)",
                cancellationToken);

            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }
}
