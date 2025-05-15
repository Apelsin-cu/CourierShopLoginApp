using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CourierShopLoginApp.Helpers
{
    public static class PasswordHelper
    {
        // Хеширует пароль с использованием SHA256
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // "x2" для шестнадцатеричного представления в нижнем регистре
                }
                return builder.ToString();
            }
        }

        // Проверяет введенный пароль с сохраненным хешем
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            string hashOfEnteredPassword = HashPassword(enteredPassword);
            // Сравнение без учета регистра (OrdinalIgnoreCase) для хешей
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfEnteredPassword, storedHash) == 0;
        }
    }
}
