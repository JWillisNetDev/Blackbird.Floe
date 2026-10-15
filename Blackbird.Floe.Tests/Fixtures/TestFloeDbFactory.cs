using Blackbird.Floe.Data;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace Blackbird.Floe.Tests.Fixtures;

public class TestFloeDbFactory : IDisposable, IDbContextFactory<FloeDb>
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FloeDb> _options;

    public TestFloeDbFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        _options = new DbContextOptionsBuilder<FloeDb>()
            .UseSqlite(_connection)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;
    }

    public FloeDb CreateDbContext()
    {
        return new FloeDb(_options);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
