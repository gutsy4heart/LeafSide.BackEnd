namespace LeafSide.Domain.Services;

public interface IJwtTokenService
{
    string GenerateToken(string userId, string email, IEnumerable<string> roles);
    string GenerateToken(object user, IEnumerable<string> roles);
}
