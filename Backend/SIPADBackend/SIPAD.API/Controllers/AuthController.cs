using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SIPAD.API.DTOs;
using SIPAD.API.Services;
using SIPAD.Infrastructure.Data;

namespace SIPAD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Datos de registro inválidos.", errors = ModelState });
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Conflict(new { success = false, message = "El correo ya está registrado." });
        }

        var user = new ApplicationUser
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            UserName = request.Email.Trim().ToLowerInvariant(),
            FullName = request.FullName.Trim(),
            EmailConfirmed = true,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(new
            {
                success = false,
                message = "No se pudo crear el usuario.",
                errors = createResult.Errors.Select(e => e.Description)
            });
        }

        await _userManager.AddToRoleAsync(user, "User");

        return Ok(new { success = true, message = "Usuario creado exitosamente." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Credenciales inválidas.", errors = ModelState });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Unauthorized(new { success = false, message = "Correo o contraseña inválidos." });
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            return Unauthorized(new { success = false, message = "Correo o contraseña inválidos." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(user, roles);

        var response = new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
        };

        return Ok(new { success = true, data = response });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { success = false, message = "Token inválido." });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new { success = false, message = "Usuario no encontrado." });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            success = true,
            data = new
            {
                user.Email,
                user.FullName,
                roles
            }
        });
    }
}
