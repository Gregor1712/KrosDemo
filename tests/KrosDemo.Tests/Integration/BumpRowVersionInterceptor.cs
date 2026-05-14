using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KrosDemo.Tests.Integration;

// SQLite has no native rowversion, so EF cannot bump it on its own. This
// interceptor mimics SQL Server's behaviour by writing a fresh monotonic
// 8-byte value into any "RowVersion" byte[] property on Added or Modified
// entries before save. Combined with SqliteRowVersionModelCustomizer (which
// flips RowVersion from DB-generated to caller-managed) it gives end-to-end
// concurrency-token semantics matching production.
internal sealed class BumpRowVersionInterceptor : SaveChangesInterceptor
{
    private static long _seq;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        Bump(eventData.Context!);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Bump(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Bump(DbContext ctx)
    {
        foreach (var entry in ctx.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            var prop = entry.Properties.FirstOrDefault(p =>
                p.Metadata.Name == "RowVersion" && p.Metadata.ClrType == typeof(byte[]));
            if (prop is null) continue;

            prop.CurrentValue = BitConverter.GetBytes(Interlocked.Increment(ref _seq));
        }
    }
}