using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Data
{
    public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
    {
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("notification_service");
            base.OnModelCreating(modelBuilder);
        }
    }
}
