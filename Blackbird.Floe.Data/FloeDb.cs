using Microsoft.EntityFrameworkCore;

namespace Blackbird.Floe.Data;

public class FloeDb : DbContext
{
    public DbSet<Question> Questions => Set<Question>();

    public FloeDb(DbContextOptions<FloeDb> options) : base(options)
    {
    }
    
}
