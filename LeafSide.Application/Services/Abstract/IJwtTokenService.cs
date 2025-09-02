using LeafSide.Infrastructure.Identity;

namespace LeafSide.Application.Services.Abstract;

public interface IJwtTokenService
{
    string GenerateToken(AppUser user, IEnumerable<string> roles);
}


