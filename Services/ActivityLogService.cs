using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services.Interfaces;
using MongoDB.Driver;

namespace LibraryManagementSystem.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IMongoCollection<ActivityLog> _logs;

    public ActivityLogService(IConfiguration config)
    {
        var connStr = config["MongoDB:ConnectionString"]!;
        var dbName = config["MongoDB:DatabaseName"] ?? "LibraryLogs";
        var client = new MongoClient(connStr);
        var database = client.GetDatabase(dbName);
        _logs = database.GetCollection<ActivityLog>("activity_logs");

        var ttlIndex = new CreateIndexModel<ActivityLog>(
            Builders<ActivityLog>.IndexKeys.Ascending(x => x.Timestamp),
            new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(90) });
        _logs.Indexes.CreateOne(ttlIndex);
    }

    public async Task LogAsync(string action, string userId, string userEmail, string role,
        string? entityType = null, string? entityId = null, string? details = null, string? ipAddress = null)
    {
        var log = new ActivityLog
        {
            Action = action,
            UserId = userId,
            UserEmail = userEmail,
            Role = role,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };
        await _logs.InsertOneAsync(log);
    }

    public async Task<List<ActivityLog>> GetLogsAsync(int page = 1, int pageSize = 50)
    {
        return await _logs.Find(_ => true)
            .SortByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<List<ActivityLog>> GetLogsByUserAsync(string userId, int page = 1, int pageSize = 50)
    {
        return await _logs.Find(x => x.UserId == userId)
            .SortByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }
}
