namespace LeafSide.API.Responses.Admin;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
}


