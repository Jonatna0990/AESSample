using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AESSample
{
    /// <summary>
    /// Класс-помощник для работы с шифрами
    /// </summary>
    public static class SecurityHelper
    {

        /// <summary>
        /// Шифрует текст по алгоритму AES
        /// </summary>
        /// <param name="value_key">Пара текст-ключ</param>
        /// <returns>Возвращает зашифрованнный текст</returns>
        public static string Encrypt(Tuple<string, string> value_key)
        {

            SHA256 mySHA256 = SHA256.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(value_key.Item2));
            byte[] iv = new byte[16] { 0x012, 0x03, 0x06, 0x04, 0x02, 0x45, 0x33, 0x02, 0x12, 0x65, 0x53, 0x043, 0x02, 0x34, 0x4, 0x65 };
            string encrypted = EncryptString(value_key.Item1, key, iv);
            return encrypted;
        }


        /// <summary>
        /// Расшивровывает текст по алгоритму AES
        /// </summary>
        /// <param name="value_key">Пара текст-ключ</param>
        /// <returns>Возвращает рашифрованный текст</returns>
        public static string Decrypt(Tuple<string, string> value_key)
        {
            SHA256 mySHA256 = SHA256.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(value_key.Item2));
            byte[] iv = new byte[16] { 0x012, 0x03, 0x06, 0x04, 0x02, 0x45, 0x33, 0x02, 0x12, 0x65, 0x53, 0x043, 0x02, 0x34, 0x4, 0x65 };
            string encrypted = DecryptString(value_key.Item1, key, iv);
            return encrypted;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="plainText">Текст для шифрования</param>
        /// <param name="key">Ключ шифрования</param>
        /// <param name="iv">Вектор инициализации</param>
        /// <returns></returns>
        private static string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            if (encryptor != null)
            {
                encryptor.Mode = CipherMode.CBC;
                encryptor.BlockSize = 128;
                // Set key and IV
                byte[] aesKey = new byte[32];
                Array.Copy(key, 0, aesKey, 0, 32);
                encryptor.Key = aesKey;
                encryptor.IV = iv;

                // Instantiate a new encryptor from our Aes object
                ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] result = null;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        result = memoryStream.ToArray();
                    }
                }
                encryptor.Clear();
                return Convert.ToBase64String(result, 0, result.Length); ;
            }
            return null;
        }

        private static string DecryptString(string cipherText, byte[] key, byte[] iv)
        {
            Aes encryptor = Aes.Create();
            if (encryptor != null)
            {
                encryptor.Mode = CipherMode.CBC;
                encryptor.BlockSize = 128;
                
                byte[] aesKey = new byte[32];
                Array.Copy(key, 0, aesKey, 0, 32);
                encryptor.Key = aesKey;
                encryptor.IV = iv;

                ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();

                string plaintext = string.Empty;
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (MemoryStream memoryStream = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(cryptoStream))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
                encryptor.Clear();
                return plaintext;
            }
            return null;

        }

        public static string CreateSHA256(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public static string CreateMD5(string input)
        {
            byte[] bytePassword = Encoding.UTF8.GetBytes(input);

            using (MD5 md5 = MD5.Create())
            {
                byte[] byteHashedPassword = md5.ComputeHash(bytePassword);
                StringBuilder result = new StringBuilder(byteHashedPassword.Length * 2);

                for (int i = 0; i < byteHashedPassword.Length; i++)
                    result.Append(byteHashedPassword[i].ToString("x2"));

                return result.ToString();
            }
        }
        
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
