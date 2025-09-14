using LeafSide.Infrastructure.Identity;
using LeafSide.API.Requests.Admin;
using LeafSide.API.Responses.Admin;
using LeafSide.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("users")]
    public async Task<ActionResult<List<UserWithRoleResponse>>> GetAllUsers()
    {
        var users = _userManager.Users.ToList();
        var userResponses = new List<UserWithRoleResponse>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userResponses.Add(new UserWithRoleResponse
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Roles = roles.ToList(),
                CreatedAt = user.Id.ToString().Length > 0 ? DateTime.UtcNow : DateTime.MinValue // Простая заглушка для даты создания
            });
        }

        return Ok(userResponses);
    }

    [HttpPut("users/{userId}/role")]
    public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateUserRoleRequest request)
    {
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return BadRequest("Invalid user ID");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Удаляем все существующие роли
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        // Добавляем новую роль
        var roleName = request.Role.ToString();
        var result = await _userManager.AddToRoleAsync(user, roleName);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = $"User role updated to {roleName}" });
    }

    [HttpGet("roles")]
    public ActionResult<List<string>> GetAvailableRoles()
    {
        var roles = Enum.GetNames(typeof(UserRole)).ToList();
        return Ok(roles);
    }
}