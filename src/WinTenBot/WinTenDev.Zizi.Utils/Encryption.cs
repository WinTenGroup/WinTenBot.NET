using System;
using System.Security.Cryptography;
using EasyEncrypt2;
using Serilog;

namespace WinTenDev.Zizi.Utils
{
    public static class Encryption
    {
        public static string Password { get; set; } = "1234";
        public static string Salt { get; set; } = "12345678";

        public static string AesEncrypt(this string input)
        {
            try
            {
                var aes = new EasyEncrypt(Password, Salt, Aes.Create());
                var result = aes.Encrypt(input);
                aes.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error AES Encrypt");
                return null;
            }
        }

        public static string AesDecrypt(this string encryptedInput)
        {
            try
            {
                var aes = new EasyEncrypt(Password, Salt, Aes.Create());
                var result = aes.Decrypt(encryptedInput);
                aes.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error AES Decrypt");
                return null;
            }
        }
    }
}