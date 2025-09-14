using LeafSide.Domain.Enums;

namespace LeafSide.API.Responses.Admin;

public class UserWithRoleResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
