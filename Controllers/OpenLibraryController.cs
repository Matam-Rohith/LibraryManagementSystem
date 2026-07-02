using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OpenLibraryController : ControllerBase
{
    private readonly IOpenLibraryService _openLibrary;

    public OpenLibraryController(IOpenLibraryService openLibrary)
    {
        _openLibrary = openLibrary;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required.");
        var results = await _openLibrary.SearchBooksAsync(query, limit);
        return Ok(results);
    }

    [HttpGet("isbn/{isbn}")]
    public async Task<IActionResult> GetByIsbn(string isbn)
    {
        var book = await _openLibrary.GetBookByIsbnAsync(isbn);
        if (book == null) return NotFound($"No book found for ISBN {isbn}");
        return Ok(book);
    }
}
