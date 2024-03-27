namespace IdentityApi.Security.Interfaces;

public interface IPasswordService
{
    void CreatePasswordHash(string password, out byte[] hash, out byte[] salt);
    bool VerifyPasswordHash(string password, byte[] hash, byte[] salt);
}