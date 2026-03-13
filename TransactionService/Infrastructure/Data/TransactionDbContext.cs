using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Data
{
    public class TransactionDbContext(DbContextOptions<TransactionDbContext> options) : DbContext(options)
    {
        public DbSet<TransactionDetails> Transactions => Set<TransactionDetails>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Chỉ định schema
            modelBuilder.HasDefaultSchema("transaction_service");
            base.OnModelCreating(modelBuilder);
        }
    }
}
