using Microsoft.AspNetCore.Identity;

namespace Web2.Repositories
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}