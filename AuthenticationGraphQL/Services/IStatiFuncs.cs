using AuthenticationGraphQL.Models;

namespace AuthenticationGraphQL.Services
{
    public interface IStatiFuncs
    {
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
        string CreateRandomToken();
        string CreateToken(UserModel user);
        (string, string) ExtractNamesFromUsername(string username);
    }
}
