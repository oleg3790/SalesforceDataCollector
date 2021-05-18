using Microsoft.EntityFrameworkCore;
using SalesforceDataCollector.Data.Models;

namespace SalesforceDataCollector.Data
{
    public class AccountContext : DbContext
    {
        public DbSet<AccountDataModel> Accounts { get; set; }

        public AccountContext(DbContextOptions<AccountContext> dbContextOptions)
            : base(dbContextOptions)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AccountDataModel>(x =>
            {
                x.Property(e => e.Created).HasDefaultValueSql("now()");
            });
        }
    }
}
