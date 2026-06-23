using LibraryManagementSystem.DTOs;
namespace LibraryManagementSystem.Services.Interfaces;
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
