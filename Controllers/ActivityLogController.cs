using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LibraryManagementSystem.Controllers;

/// <summary>Admin endpoint to retrieve MongoDB activity logs.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Tags("Activity Logs")]
public class ActivityLogController : ControllerBase
{
    private readonly IActivityLogService _logService;

    public ActivityLogController(IActivityLogService logService)
    {
        _logService = logService;
    }

    /// <summary>Get all activity logs (paginated). Admin only.</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all logs", Description = "Returns paginated activity logs from MongoDB. Admin role required.")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _logService.GetLogsAsync(page, pageSize);
        return Ok(logs);
    }

    /// <summary>Get activity logs for a specific user.</summary>
    [HttpGet("user/{userId}")]
    [SwaggerOperation(Summary = "Get logs by user")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetLogsByUser(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _logService.GetLogsByUserAsync(userId, page, pageSize);
        return Ok(logs);
    }
}
