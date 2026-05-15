using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Items)
            .WithOne(item => item.Invoice)
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.Quantity)
            .HasPrecision(18, 3);

        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.VatRate)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Type).IsRequired().HasMaxLength(512);
            entity.Property(m => m.Payload).IsRequired();
            entity.Property(m => m.OccurredOnUtc).IsRequired();
            entity.HasIndex(m => m.ProcessedOnUtc);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        if (entitiesWithEvents.Count == 0)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        var ownsTransaction = Database.CurrentTransaction is null;
        var transaction = ownsTransaction
            ? await Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);

            foreach (var entity in entitiesWithEvents)
            {
                foreach (var domainEvent in entity.DomainEvents)
                {
                    var eventType = domainEvent.GetType();
                    OutboxMessages.Add(new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        Type = eventType.AssemblyQualifiedName!,
                        Payload = JsonSerializer.Serialize(domainEvent, eventType),
                        OccurredOnUtc = domainEvent.OccurredOnUtc
                    });
                }
                entity.ClearDomainEvents();
            }

            await base.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return result;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }
}