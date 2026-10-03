using System.Security.Cryptography;

namespace AwesomePizza.Api.Service;

public static class OrderCodeGenerator
{
    // 32 uppercase characters, without the ambiguous 0/O and 1/I.
    private const string Alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const int Length = 6;

    public static string Generate() => RandomNumberGenerator.GetString(Alphabet, Length);
}
