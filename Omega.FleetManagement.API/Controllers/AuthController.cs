using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Infrastructure.Data.Context;
using Omega.FleetManagement.Infrastructure.Data.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly FleetContext _context;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration, FleetContext context)
    {
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Busca por UserName (CPF para motorista / e-mail para admin).
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null && request.Username.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(request.Username);
        }

        // 2. Valida usuário e senha
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Usuário ou senha inválidos." });

        if (!user.IsActive)
            return Unauthorized(new { message = "Usuário desativado. Procure o administrador do sistema." });

        // 3. Busca as Roles (ex: Master)
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        // 4. Gera o Token
        var token = GenerateJwtToken(user, role);

        string? companyName = null;
        if (user.CompanyId.HasValue)
        {
            companyName = await _context.Companies
                .Where(c => c.Id == user.CompanyId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
        }

        return Ok(new LoginResponse(token, user.Name, role, companyName));
    }

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Role, role),
                new Claim("UserId", user.Id.ToString()),
                new Claim("CompanyId", user.CompanyId?.ToString() ?? ""), // Null para o Master
                new Claim("FullName", user.Name)
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"]!)),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
