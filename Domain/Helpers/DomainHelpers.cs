using System.Security.Cryptography;
using System.Text;

namespace Domain.Helpers;

public static class DomainHelpers
{
    #region Encryption

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            using SHA256 sha = SHA256.Create();
            byte[] key = sha.ComputeHash(Encoding.UTF8.GetBytes("ALYVE.life"));

            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.IV = Encoding.UTF8.GetBytes("1234567890ABCDEF");

            using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();
            }

            return Convert.ToBase64String(ms.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string Decrypt(string? cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            string base64 = cipherText.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            using SHA256 sha = SHA256.Create();
            byte[] key = sha.ComputeHash(Encoding.UTF8.GetBytes("ALYVE.life"));
            byte[] cipherBytes = Convert.FromBase64String(base64);

            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.IV = Encoding.UTF8.GetBytes("1234567890ABCDEF");

            using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream ms = new(cipherBytes);
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader reader = new(cs, Encoding.UTF8);
            return reader.ReadToEnd();
        }
        catch
        {
            return string.Empty;
        }
    }

    #endregion

    #region Hashing

    public static string GetSHA(string text)
    {
        using var sha = SHA256.Create();
        byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(hashBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    #endregion
}
