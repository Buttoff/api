using Microsoft.EntityFrameworkCore;

namespace CalculationApi.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Calculation> Calculations { get; set; }
}