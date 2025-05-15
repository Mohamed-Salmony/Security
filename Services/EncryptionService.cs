using System.Security.Cryptography;
using System.Text;
using System.IO;
using System;

namespace NetMvcApp.Services
{
    public class EncryptionService
    {
        public string HashPasswordSHA256(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public (string publicKey, string privateKey) GenerateRSAKeyPair()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                // For this application, we are exporting them as Base64 strings.
                string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                // Exporting PKCS#8 private key for broader compatibility if needed, but .NET typically uses its own format.
                // For simplicity with RSACryptoServiceProvider, ExportRSAPrivateKey is fine.
                string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                return (publicKey, privateKey);
            }
        }

        public string EncryptRSA(string plainText, string publicKeyBase64)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
                rsa.ImportRSAPublicKey(publicKeyBytes, out _);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = rsa.Encrypt(plainTextBytes, true); // OAEP padding
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        public string DecryptRSA(string encryptedTextBase64, string privateKeyBase64)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
                byte[] encryptedTextBytes = Convert.FromBase64String(encryptedTextBase64);
                byte[] decryptedBytes = rsa.Decrypt(encryptedTextBytes, true); // OAEP padding
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }


        // Generates a 24-byte key for TripleDES (192-bit)
        private byte[] GenerateTripleDESKey()
        {
            using (var tripleDes = TripleDES.Create())
            {
                tripleDes.GenerateKey();
                return tripleDes.Key;
            }
        }

        // Generates an 8-byte IV for TripleDES
        private byte[] GenerateTripleDESIV()
        {
            using (var tripleDes = TripleDES.Create())
            {
                tripleDes.GenerateIV();
                return tripleDes.IV;
            }
        }

        public string EncryptTripleDES(string plainText, out string keyBase64, out string ivBase64)
        {
            byte[] key = GenerateTripleDESKey();
            byte[] iv = GenerateTripleDESIV();

            keyBase64 = Convert.ToBase64String(key);
            ivBase64 = Convert.ToBase64String(iv);

            using (TripleDES tripleDes = TripleDES.Create())
            {
                tripleDes.Key = key;
                tripleDes.IV = iv;
                tripleDes.Mode = CipherMode.CBC; 
                tripleDes.Padding = PaddingMode.PKCS7; 

                using (ICryptoTransform encryptor = tripleDes.CreateEncryptor())
                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public string DecryptTripleDES(string encryptedTextBase64, string keyBase64, string ivBase64)
        {
            byte[] key = Convert.FromBase64String(keyBase64);
            byte[] iv = Convert.FromBase64String(ivBase64);
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedTextBase64);

            using (TripleDES tripleDes = TripleDES.Create())
            {
                tripleDes.Key = key;
                tripleDes.IV = iv;
                tripleDes.Mode = CipherMode.CBC;
                tripleDes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = tripleDes.CreateDecryptor())
                using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
}

