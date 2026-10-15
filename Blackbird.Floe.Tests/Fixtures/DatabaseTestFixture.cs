using Blackbird.Floe.Data;

using Microsoft.EntityFrameworkCore;

using Moq.AutoMock;

namespace Blackbird.Floe.Tests.Fixtures;

public abstract class DatabaseTestFixture
{
    protected AutoMocker AutoMocker { get; }
    protected IDbContextFactory<FloeDb> DbContextFactory { get; }

    protected DatabaseTestFixture()
    {
        DbContextFactory = new TestFloeDbFactory();
        using (var db = DbContextFactory.CreateDbContext())
        {
            db.Database.EnsureCreated();
        }

        AutoMocker = new AutoMocker();
        AutoMocker.Use(DbContextFactory);
        AutoMocker.Use(DbContextFactory.CreateDbContext);
        AutoMocker.Use(async () => await DbContextFactory.CreateDbContextAsync());
    }
}
