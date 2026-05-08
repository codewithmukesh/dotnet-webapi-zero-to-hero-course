using System.Security.Cryptography;

namespace ApiKeyAuth.Api.Authentication;

public static class ApiKeyGenerator
{
    public static string Generate(string prefix = "sk_live_")
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);

        var secret = Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return $"{prefix}{secret}";
    }
}
