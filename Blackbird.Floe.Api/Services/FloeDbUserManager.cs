using System.Diagnostics.CodeAnalysis;

using Blackbird.Floe.Data;
using Blackbird.Floe.Data.Models.Identity;

using Microsoft.EntityFrameworkCore;

namespace Blackbird.Floe.Api.Services;

public class FloeDbUserManager : IUserManager
{
    private readonly FloeDb _db;

    public FloeDbUserManager(FloeDb db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<bool> AddAsync(User user, CancellationToken ct = default)
    {
        // normalize the email address
        user.Email = NormalizeEmail(user.Email);

        // ensure user by email doesn't already exist
        if (await EmailExistsAsync(user.Email, ct))
        {
            return false;
        }

        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _db.Users.AnyAsync(u => u.Email == NormalizeEmail(email), ct);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == NormalizeEmail(email), ct);

        return user;
    }

    public async Task<User?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var user = await _db.Users.FindAsync(id);

        return user;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        int deleted = await _db.Users
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync(ct);
        return deleted > 0;
    }

    [return: NotNullIfNotNull(nameof(email))]
    private static string? NormalizeEmail(string? email)
    {
        return email?.Trim().ToLowerInvariant();
    }
}
