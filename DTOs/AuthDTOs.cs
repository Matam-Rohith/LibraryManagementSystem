namespace LibraryManagementSystem.DTOs;

public record RegisterRequest(string FullName, string Email, string Password, string Role = "Member");
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Email, string FullName, string Role, DateTime ExpiresAt, string? UserId = null);
