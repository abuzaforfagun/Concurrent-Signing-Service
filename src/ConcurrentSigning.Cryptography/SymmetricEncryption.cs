using System.Security.Cryptography;
using System.Text;

namespace ConcurrentSigning.Cryptography;

public static class SymmetricEncryption
{
    public static string Encrypt(string plainText, string key)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = Encoding.ASCII.GetBytes(key);
        aesAlg.IV = new byte[16];

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new MemoryStream();
        using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using StreamWriter swEncrypt = new StreamWriter(csEncrypt);
        swEncrypt.Write(plainText);

        return msEncrypt.ToString();
    }

    public static string Decrypt(string cipherText, string key)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = Encoding.ASCII.GetBytes(key);
        aesAlg.IV = new byte[16];

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new MemoryStream(Encoding.ASCII.GetBytes(cipherText));
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }
}