namespace LeafSide.API.Requests.Admin;

public class UpdateUserRolesRequest
{
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
}


