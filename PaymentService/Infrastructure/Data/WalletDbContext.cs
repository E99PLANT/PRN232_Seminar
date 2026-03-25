using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Data;

public class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<WalletEvent> WalletEvents { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // QUAN TRỌNG: Schema riêng cho bài của Khánh
        modelBuilder.HasDefaultSchema("khanh_wallet");

        // === Cấu hình bảng Account ===
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.Username)
                .IsUnique();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .IsRequired();
        });

        // === Cấu hình bảng Wallet ===
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Quan hệ 1-1: Account ↔ Wallet
            entity.HasOne(w => w.Account)
                .WithOne(a => a.Wallet)
                .HasForeignKey<Wallet>(w => w.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.AccountId)
                .IsUnique();

            entity.Property(e => e.Balance)
                .HasPrecision(18, 2);

            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("VND");
        });

        // === Cấu hình bảng WalletTransaction ===
        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Quan hệ 1-N: Wallet ↔ WalletTransaction
            entity.HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.TransactionType)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);

            entity.Property(e => e.BalanceBefore)
                .HasPrecision(18, 2);

            entity.Property(e => e.BalanceAfter)
                .HasPrecision(18, 2);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.SuspiciousReason)
                .HasMaxLength(1000);

            // Index cho tra cứu bất thường nhanh
            entity.HasIndex(e => e.IsSuspicious);
            entity.HasIndex(e => e.Timestamp);
        });

        // === Cấu hình bảng WalletEvent (Event Sourcing) ===
        modelBuilder.Entity<WalletEvent>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.AggregateType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.EventData)
                .IsRequired();

            // Index cho Event Sourcing queries
            entity.HasIndex(e => e.AggregateId);
            entity.HasIndex(e => e.Timestamp);

            // Hash chain cho integrity verification
            entity.Property(e => e.Hash)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.PreviousHash)
                .HasMaxLength(64);
        });

        // === Cấu hình bảng OutboxMessage (Outbox Pattern) ===
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.EventData)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.HasIndex(e => e.IsSent);
        });
    }
}
