namespace Identity.Api.Security.Interfaces;

public interface IPasswordManager
{
    void CreatePasswordHash(string password, out byte[] hash, out byte[] salt);
    bool VerifyPasswordHash(string password, byte[] hash, byte[] salt);
}