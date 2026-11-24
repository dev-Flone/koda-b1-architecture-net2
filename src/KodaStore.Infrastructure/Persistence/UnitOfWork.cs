using System.Data;
using KodaStore.Application.Abstractions.Persistence;
using KodaStore.Application.Common;
using KodaStore.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace KodaStore.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext context, ILogger logger) : IUnitOfWork
{
    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        return new UnitOfWorkTransaction(transaction);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException e)
        {
            logger.LogWarning(e, "Concurrency Conflict");

            throw new DataConcurrencyException("Data telah berubah oleh request lain.", e);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new UniqueConstraintViolationException("Data dengan nilai unik sudah tersedia", e);
        }
    }

    // HELPER
    private sealed class UnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken) => transaction.CommitAsync(cancellationToken);

        public Task RollBackAsync(CancellationToken cancellationToken) => transaction.RollbackAsync(cancellationToken);
    }
}
