using Microsoft.EntityFrameworkCore;

namespace Stockservice.Data;

public class StockDbContext : DbContext
{
    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options)
    {
    }
}
