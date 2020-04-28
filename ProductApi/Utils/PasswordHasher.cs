using System;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using ProductApi.Models;
using System.Security.Cryptography;

namespace ProductApi.Utils
{
    public static class PasswordHasher
    {
        const int subkeySize = 16;
        const int iterationsCount = 1000;
        const int saltSize = 16;

        private static byte[] HashPassword(string password, byte[] salt)
        {
            if (salt.Length != saltSize)
            {
                throw new ArgumentException("Salt size don't match");
            }

            byte[] hashedPassword = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA1,
                iterationsCount,
                subkeySize);

            byte[] hashedPasswordWithSalt = new byte[saltSize + subkeySize];
            hashedPassword.CopyTo(hashedPasswordWithSalt, saltSize);
            salt.CopyTo(hashedPasswordWithSalt, 0);
            return hashedPasswordWithSalt;
        }

        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[saltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private static bool ComparePasswords(string providedPassword, User user)
        {
            byte[] salt = new byte[saltSize];
            byte[] passwordHash = Convert.FromBase64String(user.Password);
            Array.Copy(passwordHash, 0, salt, 0, saltSize);
            byte[] providedPasswordHash = HashPassword(providedPassword, salt);
            return Convert.ToBase64String(providedPasswordHash).CompareTo(user.Password) == 0;
        }
    }
}
