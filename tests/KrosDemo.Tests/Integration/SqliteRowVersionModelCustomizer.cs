using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KrosDemo.Tests.Integration;

// IsRowVersion() in OnModelCreating marks RowVersion as DB-generated, so EF
// never includes it in INSERTs. SQL Server fills it in, but SQLite cannot —
// every insert hits NOT NULL. This customizer runs after the DbContext's own
// OnModelCreating and downgrades RowVersion to "regular byte[] managed by the
// caller" while keeping the concurrency-token semantics intact.
internal sealed class SqliteRowVersionModelCustomizer : RelationalModelCustomizer
{
    public SqliteRowVersionModelCustomizer(ModelCustomizerDependencies dependencies)
        : base(dependencies) { }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var prop = entityType.FindProperty("RowVersion");
            if (prop is null || prop.ClrType != typeof(byte[])) continue;

            prop.ValueGenerated = ValueGenerated.Never;
            prop.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
            prop.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        }
    }
}