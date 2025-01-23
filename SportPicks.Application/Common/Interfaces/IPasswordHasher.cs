namespace Application.Common.Interfaces;

public interface IPasswordHasher
{
    (string HashedPassword, string Salt) HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword, string salt);
}
