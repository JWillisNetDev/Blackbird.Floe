using Blackbird.Floe.Data.Models;
using Blackbird.Floe.Data.Models.Identity;

using Microsoft.EntityFrameworkCore;

namespace Blackbird.Floe.Data;

public class FloeDb : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Question> Questions => Set<Question>();
    
    public FloeDb(DbContextOptions<FloeDb> options) : base(options)
    {
    }    
}
