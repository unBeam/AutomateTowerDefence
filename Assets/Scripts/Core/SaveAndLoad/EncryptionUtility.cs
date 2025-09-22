using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class EncryptionUtility
{
    private readonly byte[] EncryptionKey = Convert.FromBase64String("AN5e6TATQ6piSM/DzjWqqQ==");

    public string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            aes.GenerateIV();

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                string encryptedData = Convert.ToBase64String(ms.ToArray());
                return "ENC:" + encryptedData;
            }
        }
    }

    public string Decrypt(string cipherText)
    {
        if (!cipherText.StartsWith("ENC:"))
        {
            return cipherText;
        }

        cipherText = cipherText.Substring(4);
        byte[] fullCipher = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];

            Array.Copy(fullCipher, iv, iv.Length);
            Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);
            aes.IV = iv;

            using (MemoryStream ms = new MemoryStream(cipher))
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
