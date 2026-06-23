using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinesController : ControllerBase
{
    private readonly IFineService _fines;
    public FinesController(IFineService fines) => _fines = fines;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() => Ok(await _fines.GetAllAsync());

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId) => Ok(await _fines.GetByUserIdAsync(userId));

    [HttpPost("pay")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Pay(PayFineRequest req) => Ok(await _fines.PayFineAsync(req));
}
