using System.Security.Cryptography;

namespace SchoolEquipmentManagement.Infrastructure.Security
{
    public static class PasswordHashUtility
    {
        private const string Algorithm = "PBKDF2";
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int DefaultIterations = 100_000;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));
            }

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, DefaultIterations, HashAlgorithmName.SHA256, KeySize);

            return string.Join(
                '$',
                Algorithm,
                DefaultIterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            {
                return false;
            }

            var parts = passwordHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4 ||
                !string.Equals(parts[0], Algorithm, StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(parts[1], out var iterations))
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);
                var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
