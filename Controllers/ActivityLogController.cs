using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ActivityLogController : ControllerBase
{
    private readonly IActivityLogService _logService;

    public ActivityLogController(IActivityLogService logService)
    {
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _logService.GetLogsAsync(page, pageSize);
        return Ok(logs);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetLogsByUser(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _logService.GetLogsByUserAsync(userId, page, pageSize);
        return Ok(logs);
    }
}
