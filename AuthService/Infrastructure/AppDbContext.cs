using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<EventStoreRecord> EventStoreRecords => Set<EventStoreRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("customer_auth");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Accounts");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Email).IsRequired().HasMaxLength(255);
                entity.Property(x => x.PasswordHash).IsRequired();

                entity.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<EventStoreRecord>(entity =>
            {
                entity.ToTable("EventStoreRecords");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.AggregateId).IsRequired().HasMaxLength(50);
                entity.Property(x => x.AggregateType).IsRequired().HasMaxLength(100);
                entity.Property(x => x.EventType).IsRequired().HasMaxLength(200);
                entity.Property(x => x.EventData).IsRequired();

                entity.HasIndex(x => x.AggregateId);
            });
        }
    }
}
