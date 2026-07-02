using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services.Interfaces;

public interface IActivityLogService
{
    Task LogAsync(string action, string userId, string userEmail, string role,
        string? entityType = null, string? entityId = null, string? details = null, string? ipAddress = null);
    Task<List<ActivityLog>> GetLogsAsync(int page = 1, int pageSize = 50);
    Task<List<ActivityLog>> GetLogsByUserAsync(string userId, int page = 1, int pageSize = 50);
}
