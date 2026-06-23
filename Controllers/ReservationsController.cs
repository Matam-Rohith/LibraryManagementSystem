using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _res;
    public ReservationsController(IReservationService res) => _res = res;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() => Ok(await _res.GetAllAsync());

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId) => Ok(await _res.GetByUserIdAsync(userId));

    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationRequest req) => Ok(await _res.CreateAsync(req));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id) { await _res.CancelAsync(id); return NoContent(); }
}
