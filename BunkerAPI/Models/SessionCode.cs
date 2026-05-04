using System.Security.Cryptography;

namespace BunkerAPI.Models;

public static class SessionCode
{
    public const int Length = 6;

    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /// <summary>Нормализует ввод: ровно 6 символов A–Z / 0–9 (регистр букв не важен).</summary>
    public static bool TryNormalize(string? raw, out string code)
    {
        code = "";
        if (string.IsNullOrWhiteSpace(raw) || raw.Length != Length)
            return false;

        Span<char> buf = stackalloc char[Length];
        for (var i = 0; i < Length; i++)
        {
            var c = char.ToUpperInvariant(raw[i]);
            var letter = c is >= 'A' and <= 'Z';
            var digit = c is >= '0' and <= '9';
            if (!letter && !digit)
                return false;
            buf[i] = c;
        }

        code = new string(buf);
        return true;
    }

    public static string GenerateNew()
    {
        Span<byte> rnd = stackalloc byte[Length];
        RandomNumberGenerator.Fill(rnd);
        Span<char> chars = stackalloc char[Length];
        for (var i = 0; i < Length; i++)
            chars[i] = Alphabet[rnd[i] % Alphabet.Length];

        return new string(chars);
    }
}
