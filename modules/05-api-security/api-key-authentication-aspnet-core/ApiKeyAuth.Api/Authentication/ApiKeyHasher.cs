using System.Security.Cryptography;
using System.Text;

namespace ApiKeyAuth.Api.Authentication;

public static class ApiKeyHasher
{
    public static string Hash(string plaintextKey)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(plaintextKey), hash);
        return Convert.ToHexString(hash);
    }

    public static bool Verify(string plaintextKey, string storedHash)
    {
        var computed = Hash(plaintextKey);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(storedHash));
    }
}
