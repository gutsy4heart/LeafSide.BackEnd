using LeafSide.API.Requests.Admin;
using LeafSide.API.Responses.Admin;
using LeafSide.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public AdminUsersController(UserManager<AppUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
    {
        var users = _userManager.Users.ToList();
        var result = new List<UserResponse>(users.Count);
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new UserResponse { Id = u.Id, Email = u.Email ?? string.Empty, Roles = roles });
        }
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById([FromForm]Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new UserResponse { Id = user.Id, Email = user.Email ?? string.Empty, Roles = roles });
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> UpdateRoles([FromForm]Guid id, [FromForm] UpdateUserRolesRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        var current = await _userManager.GetRolesAsync(user);
        var toRemove = current.Except(request.Roles).ToArray();
        var toAdd = request.Roles.Except(current).ToArray();

        if (toRemove.Length > 0)
            await _userManager.RemoveFromRolesAsync(user, toRemove);

        foreach (var role in toAdd)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        if (toAdd.Length > 0)
            await _userManager.AddToRolesAsync(user, toAdd);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromForm]Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }
}


