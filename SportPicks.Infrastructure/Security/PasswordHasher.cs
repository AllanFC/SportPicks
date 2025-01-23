using Application.Common.Interfaces;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public (string HashedPassword, string Salt) HashPassword(string password)
    {
        // Generate a random salt
        var salt = GenerateRandomSalt();

        // Hash the password with Argon2
        var hashedPassword = HashWithArgon2(password, salt);

        return (hashedPassword, Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string password, string hashedPassword, string salt)
    {
        // Decode the salt
        var saltBytes = Convert.FromBase64String(salt);

        // Hash the provided password with the same salt
        var computedHash = HashWithArgon2(password, saltBytes);

        // Compare the computed hash with the stored hash
        return hashedPassword == computedHash;
    }

    private string HashWithArgon2(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 8,  // Number of threads to use
            MemorySize = 65536,      // Amount of memory (in KB)
            Iterations = 4           // Number of iterations
        };

        // Hash the password
        var hashBytes = argon2.GetBytes(32); // 32-byte hash (256 bits)
        return Convert.ToBase64String(hashBytes);
    }

    private byte[] GenerateRandomSalt()
    {
        var salt = new byte[16]; // 128-bit salt
        RandomNumberGenerator.Fill(salt);
        return salt;
    }
}
