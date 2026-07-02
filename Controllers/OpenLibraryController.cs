using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LibraryManagementSystem.Controllers;

/// <summary>Search external Open Library API for book metadata.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Open Library Integration")]
public class OpenLibraryController : ControllerBase
{
    private readonly IOpenLibraryService _openLibrary;

    public OpenLibraryController(IOpenLibraryService openLibrary)
    {
        _openLibrary = openLibrary;
    }

    /// <summary>Search books from Open Library by title, author, or keyword.</summary>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Search Open Library", Description = "Queries the Open Library public API to find book metadata.")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required.");
        var results = await _openLibrary.SearchBooksAsync(query, limit);
        return Ok(results);
    }

    /// <summary>Look up book metadata by ISBN from Open Library.</summary>
    [HttpGet("isbn/{isbn}")]
    [SwaggerOperation(Summary = "Get book by ISBN", Description = "Fetches book details from Open Library using ISBN.")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByIsbn(string isbn)
    {
        var book = await _openLibrary.GetBookByIsbnAsync(isbn);
        if (book == null) return NotFound($"No book found for ISBN {isbn}");
        return Ok(book);
    }
}
