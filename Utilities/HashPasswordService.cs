using System.Security.Cryptography;

namespace SSToDo.Utilities
{
    public interface IHashPasswordService
    {
        string Hash(string password);
        bool Verify(string password, string storedPassword);
    }

    public class HashPasswordService : IHashPasswordService
    {
        private readonly int _iterations = 100_000;

        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new ArgumentException("Password format is wrong.");

            byte[] salt = RandomNumberGenerator.GetBytes(16);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, _iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            string saltB64 = Convert.ToBase64String(salt);
            string hashB64 = Convert.ToBase64String(hash);

            return $"pbkdf2${_iterations}${saltB64}${hashB64}";
        }

        public bool Verify(string password, string storedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;
            if (string.IsNullOrEmpty(storedPassword))
                return false;

            var part = storedPassword.Split('$');
            if (part.Length != 4)
                return false;

            if (!int.TryParse(part[1], out int iterations))
                return false;

            byte[] salt, storedHash;
            try
            {
                salt = Convert.FromBase64String(part[2]);
                storedHash = Convert.FromBase64String(part[3]);
            }
            catch
            {
                return false;
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(storedHash.Length);

            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
    }
}
