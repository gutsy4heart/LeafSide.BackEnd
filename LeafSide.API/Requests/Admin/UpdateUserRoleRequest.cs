using LeafSide.Domain.Enums;

namespace LeafSide.API.Requests.Admin;

public class UpdateUserRoleRequest
{
    public string UserId { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
