using System.Security.Cryptography;

namespace Blackbird.Floe.Api.Services.Auth;

public class PasswordHashService
{
    private const int SaltSizeBytes = 16;
    private const int KeySizeBytes = 32;
    private const int Iterations = 600_000;
    private static readonly HashAlgorithmName AlgorithmName = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, AlgorithmName, KeySizeBytes);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string hash)
    {
        if (hash.Split('.') is not [var iterationsRaw, var saltRaw, var keyRaw]
            || !int.TryParse(iterationsRaw, out int iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(saltRaw);
        var expectedKey = Convert.FromBase64String(keyRaw);
        var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, AlgorithmName, expectedKey.Length);
        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }
}