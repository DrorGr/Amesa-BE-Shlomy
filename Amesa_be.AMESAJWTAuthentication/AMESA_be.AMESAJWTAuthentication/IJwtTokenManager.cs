using System.Security.Claims;

namespace AMESA_be.AMESAJWTAuthentication
{
    public interface IJwtTokenManager
    {
        string GenerateAccessToken(IEnumerable<Claim> authClaims, DateTime tokenExpiration);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        string GenerateRefreshToken();
    }
}
