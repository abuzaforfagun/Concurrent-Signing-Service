using System.Security.Cryptography;
using System.Text;

namespace ConcurrentSigning.Cryptography;

public static class SymmetricEncryption
{
    public static string Encrypt(string plainText, string key)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = Encoding.ASCII.GetBytes(key);
        aesAlg.GenerateIV(); // Generate a random IV for each encryption

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new MemoryStream();
        using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using StreamWriter swEncrypt = new StreamWriter(csEncrypt);
        swEncrypt.Write(plainText);

        swEncrypt.Flush();
        csEncrypt.FlushFinalBlock();

        byte[] ivAndEncryptedText = new byte[aesAlg.IV.Length + msEncrypt.Length];
        Buffer.BlockCopy(aesAlg.IV, 0, ivAndEncryptedText, 0, aesAlg.IV.Length);
        Buffer.BlockCopy(msEncrypt.ToArray(), 0, ivAndEncryptedText, aesAlg.IV.Length, (int)msEncrypt.Length);

        return Convert.ToBase64String(ivAndEncryptedText);
    }

    public static string Decrypt(string cipherText, string key)
    {
        byte[] ivAndEncryptedText = Convert.FromBase64String(cipherText);

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = Encoding.ASCII.GetBytes(key);
        byte[] iv = new byte[aesAlg.IV.Length];
        Buffer.BlockCopy(ivAndEncryptedText, 0, iv, 0, aesAlg.IV.Length);
        aesAlg.IV = iv;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new MemoryStream(ivAndEncryptedText, aesAlg.IV.Length, ivAndEncryptedText.Length - aesAlg.IV.Length);
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }
}