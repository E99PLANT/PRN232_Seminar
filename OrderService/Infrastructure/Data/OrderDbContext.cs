using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        // Bảng 1: Read Model (Trạng thái đơn hàng)
        public DbSet<Order> Orders { get; set; }

        // Bảng 2: Event Store (Sổ cái lưu lịch sử sự kiện của đơn)
        public DbSet<OrderEvent> OrderEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // QUAN TRỌNG NHẤT: Tạo một "vương quốc" (Schema) độc lập cho Order
            modelBuilder.HasDefaultSchema("danh_order");

            // Cấu hình bảng Event Store (Dùng JSONB để chứa cục Payload cực xịn của PostgreSQL)
            modelBuilder.Entity<OrderEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventData).HasColumnType("jsonb");
            });

            // Cấu hình bảng Order cơ bản
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}