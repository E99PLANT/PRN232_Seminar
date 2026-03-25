using Microsoft.EntityFrameworkCore;
using UserBehaviorService.Domain.Entities;

namespace UserBehaviorService.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserBehaviorProjection> UserBehaviorProjections => Set<UserBehaviorProjection>();
        public DbSet<UserLoginHistory> UserLoginHistories => Set<UserLoginHistory>();
        public DbSet<ConsumedMessage> ConsumedMessages => Set<ConsumedMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserBehaviorProjection>(entity =>
            {
                entity.ToTable("UserBehaviorProjections", "customer_behavior");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.UserId).IsRequired().HasMaxLength(50);
                entity.Property(x => x.Email).HasMaxLength(255);
                entity.Property(x => x.CurrentStatus).IsRequired().HasMaxLength(50);
                entity.Property(x => x.MostActiveWeekday).HasMaxLength(20);

                entity.HasIndex(x => x.UserId).IsUnique();
            });

            modelBuilder.Entity<UserLoginHistory>(entity =>
            {
                entity.ToTable("UserLoginHistories", "customer_behavior");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.UserId).IsRequired().HasMaxLength(50);
                entity.Property(x => x.Email).HasMaxLength(255);
                entity.Property(x => x.Weekday).IsRequired().HasMaxLength(20);

                entity.HasIndex(x => x.UserId);
            });

            modelBuilder.Entity<ConsumedMessage>(entity =>
            {
                entity.ToTable("ConsumedMessages", "customer_behavior");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.MessageId).IsRequired().HasMaxLength(200);
                entity.Property(x => x.EventType).IsRequired().HasMaxLength(100);

                entity.HasIndex(x => x.MessageId).IsUnique();
            });
        }
    }
}
