using System.Reflection;
using Microsoft.EntityFrameworkCore;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) {}

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Sessions> Sessions { get; set; }
    public DbSet<SessionsView> SessionsViews { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}