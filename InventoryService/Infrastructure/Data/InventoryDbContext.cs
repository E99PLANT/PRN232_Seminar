using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        // Bảng 1: Sổ cái lưu lịch sử sự kiện (Event Store)
        public DbSet<InventoryEvent> InventoryEvents { get; set; }

        // Bảng 2: Lưu trạng thái tồn kho hiện tại (Read Model)
        public DbSet<Inventory> Inventory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // QUAN TRỌNG: Đặt schema riêng cho bạn 
            modelBuilder.HasDefaultSchema("danh_inventory");

            // Cấu hình bảng EventStore
            modelBuilder.Entity<InventoryEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Lưu Data dưới dạng JSONB
                entity.Property(e => e.EventData).HasColumnType("jsonb");
            });

            // Cấu hình bảng Tồn kho hiện tại
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Mỗi sản phẩm chỉ có 1 dòng tồn kho duy nhất
                entity.HasIndex(e => e.ProductId).IsUnique();
            });
        }
    }
}