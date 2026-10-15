using Blackbird.Floe.Api.Services;
using Blackbird.Floe.Data.Models.Identity;
using Blackbird.Floe.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;

using Moq.AutoMock;

namespace Blackbird.Floe.Tests.Api.Services;

public class FloeDbUserManagerTests : DatabaseTestFixture
{
    private FloeDbUserManager CreateUserManager()
        => AutoMocker.CreateInstance<FloeDbUserManager>();

    [Fact]
    public async Task AddAsync_Works()
    {
        var sut = CreateUserManager();
        User newUser = new("test-email@mail.com", "test password hash");

        bool success = await sut.AddAsync(newUser);

        Assert.True(success);
        using var db = await DbContextFactory.CreateDbContextAsync();
        var actualUser = Assert.Single(db.Users);
        Assert.Equal("test-email@mail.com", actualUser.Email);
        Assert.Equal("test password hash", actualUser.PasswordHash);
    }

    [Fact]
    public async Task AddAsync_NormalizesEmails()
    {
        const string NotNormalizedEmail = "NoT-a-NoRmAlIzEd-EmAiL@gMaIl.com";
        const string ExpectedEmail = "not-a-normalized-email@gmail.com";
        var sut = CreateUserManager();
        User newUser = new(NotNormalizedEmail, "asdf");

        bool success = await sut.AddAsync(newUser);

        Assert.True(success);
        using var db = await DbContextFactory.CreateDbContextAsync();
        var actualUser = Assert.Single(db.Users);
        Assert.Equal(ExpectedEmail, actualUser.Email);
    }

    [Fact]
    public async Task AddAsync_FalseOnDuplicateEmail()
    {
        const string DuplicateEmail = "some-email@gmail.com";
        var sut = CreateUserManager();
        User newUser = new(DuplicateEmail, "asdf");
        using var db = await DbContextFactory.CreateDbContextAsync();
        await db.Users.AddAsync(new User(DuplicateEmail, "asdf"));
        await db.SaveChangesAsync();

        bool success = await sut.AddAsync(newUser);

        Assert.False(success);
        Assert.Single(db.Users);
    }

    [Fact]
    public async Task EmailExistsAsync_Works()
    {
        const string EmailExists = "some-email@gamil.com";
        const string EmailNotExists = "some-other-email@gmail.com";
        var sut = CreateUserManager();
        using var db = await DbContextFactory.CreateDbContextAsync();
        await db.Users.AddAsync(new User(EmailExists, "asdf"));
        await db.SaveChangesAsync();

        bool exists = await sut.EmailExistsAsync(EmailExists);
        bool notExists = await sut.EmailExistsAsync(EmailNotExists);

        Assert.True(exists);
        Assert.False(notExists);
    }

    [Fact]
    public async Task FindByEmailAsync_Works()
    {
        const string EmailExists = "some-email@gmail.com";
        const string EmailNotExists = "some-other-email@gmail.com";
        var sut = CreateUserManager();
        using var db = await DbContextFactory.CreateDbContextAsync();
        var user = new User(EmailExists, "asdf");
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var userFound = await sut.FindByEmailAsync(EmailExists);
        var userNotFound = await sut.FindByEmailAsync(EmailNotExists);

        Assert.NotNull(userFound);
        Assert.Equal(user.Id, userFound.Id);
        Assert.Null(userNotFound);
    }

    [Fact]
    public async Task FindByIdAsync_Works()
    {
        
        var sut = CreateUserManager();
        using var db = await DbContextFactory.CreateDbContextAsync();
        var user = new User("some-email@gmail.com", "asdf");
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var userFound = await sut.FindByIdAsync(user.Id);
        var userNotFound = await sut.FindByIdAsync("this isn't even a GUID");

        Assert.NotNull(userFound);
        Assert.Equal(user.Id, userFound.Id);
        Assert.Null(userNotFound);
    }

    [Fact]
    public async Task DeleteAsync_Works()
    {
        var sut = CreateUserManager();
        using var db = await DbContextFactory.CreateDbContextAsync();
        var deleteUser = new User("delete-me@gmail.com", "asdf");
        await db.Users.AddRangeAsync([deleteUser, new User("dont-delete-me@gmail.com", "asdf")]);
        await db.SaveChangesAsync();
        
        var deleted = await sut.DeleteAsync(deleteUser.Id);

        Assert.True(deleted);
        var exists = await db.Users.AnyAsync(u => u.Id == deleteUser.Id);
        Assert.False(exists);
        Assert.Single(db.Users);
    }
}
