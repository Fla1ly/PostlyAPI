using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace postly
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return $"{Convert.ToBase64String(salt)}:{hashedPassword}";
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string[] parts = hashedPassword.Split(':');
            byte[] salt = Convert.FromBase64String(parts[0]);
            string storedHash = parts[1];

            string hashedPasswordToVerify = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return hashedPasswordToVerify == storedHash;
        }

    }
}
