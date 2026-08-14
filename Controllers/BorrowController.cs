using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BorrowController : ControllerBase
{
    private readonly IBorrowService _borrow;
    public BorrowController(IBorrowService borrow) => _borrow = borrow;

    [HttpGet]
    [Authorize(Roles = "Admin,Assistant")]
    public async Task<IActionResult> GetAll() => Ok(await _borrow.GetAllAsync());

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId) => Ok(await _borrow.GetByUserIdAsync(userId));

    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,Assistant")]
    public async Task<IActionResult> GetOverdue() => Ok(await _borrow.GetOverdueAsync());

    [HttpPost("issue")]
    [Authorize(Roles = "Admin,Assistant")]
    public async Task<IActionResult> Issue(IssueBookRequest req) => Ok(await _borrow.IssueBookAsync(req));

    [HttpPost("return")]
    [Authorize(Roles = "Admin,Assistant")]
    public async Task<IActionResult> Return(ReturnBookRequest req) => Ok(await _borrow.ReturnBookAsync(req));
}
