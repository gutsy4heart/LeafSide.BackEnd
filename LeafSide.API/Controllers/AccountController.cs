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
    public async Task<IActionResult> Register([FromForm] RegisterRequest request)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            CountryCode = request.CountryCode,
            Gender = request.Gender,
            CreatedAt = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        
        // Присваиваем роль пользователя по умолчанию
        await _userManager.AddToRoleAsync(user, UserRole.User.ToString());
        
        return Ok();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromForm] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Unauthorized();
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid) return Unauthorized();
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.GenerateToken(user, roles);
        return Ok(new LoginResponse { Token = token });
    }
}


