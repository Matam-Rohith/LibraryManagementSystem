using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryManagementSystem.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _config;

    public AuthService(UserManager<User> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
    {
        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email,
            UserName = req.Email,
            MembershipId = $"LIB{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
        };

        var result = await _userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        var role = req.Role == "Admin" ? "Admin" : "Member";
        await _userManager.AddToRoleAsync(user, role);
        return await GenerateToken(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var user = await _userManager.FindByEmailAsync(req.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!await _userManager.CheckPasswordAsync(user, req.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return await GenerateToken(user);
    }

    private async Task<AuthResponse> GenerateToken(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var expiry = DateTime.UtcNow.AddHours(24);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new("FullName", user.FullName),
            new(ClaimTypes.Role, roles.FirstOrDefault() ?? "Member")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Email!, user.FullName,
            roles.FirstOrDefault() ?? "Member", expiry);
    }
}
