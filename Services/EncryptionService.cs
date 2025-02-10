using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Services
{

    public class EncryptionService
    {
        private readonly byte[] key;
        private readonly byte[] iv;

        public EncryptionService(string keyString)
        {
            // Ensure the key is 32 bytes for AES-256
            key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
            iv = new byte[16]; // Initialization vector (IV) should be 16 bytes for AES
        }


        public string Encrypt(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}