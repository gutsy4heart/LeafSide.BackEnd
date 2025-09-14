using Microsoft.AspNetCore.Identity;

namespace LeafSide.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


