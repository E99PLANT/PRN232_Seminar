using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationService.Infrastructure.Data
{
    public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
    {
        public NotificationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();

            var connectionString = "User Id=postgres.apltekcfbbfhcsvshczu;Password=Vothanhdanh@2004;Server=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Ssl Mode=Require;Trust Server Certificate=true;";

            optionsBuilder.UseNpgsql(connectionString);

            return new NotificationDbContext(optionsBuilder.Options);
        }
    }
}
