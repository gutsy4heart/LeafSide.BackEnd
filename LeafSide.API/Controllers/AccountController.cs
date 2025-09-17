using LeafSide.Infrastructure.Identity;
using LeafSide.API.Requests.Account;
using LeafSide.API.Responses.Account;
using LeafSide.Domain.Services;
using LeafSide.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        
        // Присваиваем роль пользователя по умолчанию
        await _userManager.AddToRoleAsync(user, UserRole.User.ToString());
        
        return Ok();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Unauthorized();
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid) return Unauthorized();
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.GenerateToken(user, roles);
        return Ok(new LoginResponse { Token = token });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        try
        {
            // Получаем ID пользователя из JWT токена
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"Profile endpoint: Found NameIdentifier: {userId}");
            
            if (string.IsNullOrEmpty(userId))
            {
                // Попробуем альтернативный способ получения ID
                userId = User.FindFirst("sub")?.Value;
                Console.WriteLine($"Profile endpoint: Found sub claim: {userId}");
                
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("Profile endpoint: No user ID found in token");
                    return Unauthorized("User ID not found in token");
                }
            }

            Console.WriteLine($"Profile endpoint: Looking for user with ID: {userId}");
            
            // Проверяем, что ID является валидным GUID
            if (!Guid.TryParse(userId, out var userGuid))
            {
                Console.WriteLine($"Profile endpoint: Invalid GUID format: {userId}");
                return BadRequest("Invalid user ID format");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) 
            {
                Console.WriteLine($"Profile endpoint: User not found with ID: {userId}");
                return NotFound("User not found");
            }

            Console.WriteLine($"Profile endpoint: Found user: {user.Email}");

            var roles = await _userManager.GetRolesAsync(user);
            Console.WriteLine($"Profile endpoint: User roles: {string.Join(", ", roles)}");
            
            var response = new UserProfileResponse
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                CountryCode = user.CountryCode ?? string.Empty,
                Gender = user.Gender ?? string.Empty,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt
            };
            
            Console.WriteLine($"Profile endpoint: Returning response for {response.Email}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Profile endpoint: Exception occurred: {ex.Message}");
            Console.WriteLine($"Profile endpoint: Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            // Получаем ID пользователя из JWT токена
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized("User ID not found in token");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) 
            {
                return NotFound("User not found");
            }

            // Обновляем поля профиля
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.CountryCode = request.CountryCode ?? user.CountryCode;
            user.Gender = request.Gender ?? user.Gender;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            var response = new UserProfileResponse
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                CountryCode = user.CountryCode ?? string.Empty,
                Gender = user.Gender ?? string.Empty,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest("Token is required");
            }

            var newToken = _jwtTokenService.RefreshToken(request.Token);
            return Ok(new RefreshTokenResponse { Token = newToken });
        }
        catch (ArgumentException ex)
        {
            return Unauthorized(new { error = "Invalid token", details = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    // [HttpDelete("users/{userId}")]
    // [Authorize(Roles = "Admin")]
    // public async Task<IActionResult> DeleteUser(string userId)
    // {
    //     try
    //     {
    //         var user = await _userManager.FindByIdAsync(userId);
    //         if (user is null) return NotFound("Пользователь не найден");

    //         var result = await _userManager.DeleteAsync(user);
    //         if (!result.Succeeded) return BadRequest(result.Errors);

    //         return Ok(new { message = "Пользователь успешно удален" });
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, new { error = "Внутренняя ошибка сервера", details = ex.Message });
    //     }
    // }
}

