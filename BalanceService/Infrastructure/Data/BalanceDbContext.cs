using BalanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BalanceService.Infrastructure.Data
{
    public class BalanceDbContext(DbContextOptions<BalanceDbContext> options) : DbContext(options)
    {
        public DbSet<UserBalance> Balances => Set<UserBalance>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("balance_service");
            base.OnModelCreating(modelBuilder);
        }
    }
}
