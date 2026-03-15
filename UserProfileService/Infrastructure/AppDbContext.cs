using Microsoft.EntityFrameworkCore;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<EventStoreRecord> EventStoreRecords => Set<EventStoreRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("UserProfiles", "customer_profile");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.AccountId).IsRequired().HasMaxLength(50);
                entity.Property(x => x.Email).IsRequired().HasMaxLength(255);
                entity.Property(x => x.FullName).HasMaxLength(255);
                entity.Property(x => x.Gender).HasMaxLength(50);
                entity.Property(x => x.Address).HasMaxLength(500);

                entity.HasIndex(x => x.AccountId).IsUnique();
                entity.HasIndex(x => x.Email);
            });

            modelBuilder.Entity<EventStoreRecord>(entity =>
            {
                entity.ToTable("EventStoreRecords", "customer_profile");
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
