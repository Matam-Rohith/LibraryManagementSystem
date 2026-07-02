using System.Text.Json;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryManagementSystem.Services;

public class OpenLibraryService : IOpenLibraryService
{
    private readonly HttpClient _http;
    private readonly ILogger<OpenLibraryService> _logger;

    public OpenLibraryService(HttpClient http, ILogger<OpenLibraryService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<OpenLibraryBook>> SearchBooksAsync(string query, int limit = 10)
    {
        try
        {
            var url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(query)}&limit={limit}&fields=title,author_name,first_publish_year,isbn,subject,cover_i";
            var resp = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(resp);
            var docs = doc.RootElement.GetProperty("docs");
            var results = new List<OpenLibraryBook>();
            foreach (var item in docs.EnumerateArray())
            {
                var title = item.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                var authors = item.TryGetProperty("author_name", out var a)
                    ? a.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                    : new List<string>();
                var year = item.TryGetProperty("first_publish_year", out var y) ? (int?)y.GetInt32() : null;
                var isbn = item.TryGetProperty("isbn", out var i) ? i.EnumerateArray().FirstOrDefault().GetString() : null;
                var subject = item.TryGetProperty("subject", out var s) ? s.EnumerateArray().FirstOrDefault().GetString() : null;
                var coverId = item.TryGetProperty("cover_i", out var c) ? (long?)c.GetInt64() : null;
                var coverUrl = coverId.HasValue ? $"https://covers.openlibrary.org/b/id/{coverId}-M.jpg" : null;
                results.Add(new OpenLibraryBook(title, authors, year, coverUrl, isbn, subject));
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Open Library search failed for query: {Query}", query);
            return new List<OpenLibraryBook>();
        }
    }

    public async Task<OpenLibraryBook?> GetBookByIsbnAsync(string isbn)
    {
        try
        {
            var url = $"https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data";
            var resp = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(resp);
            var key = $"ISBN:{isbn}";
            if (!doc.RootElement.TryGetProperty(key, out var book)) return null;
            var title = book.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
            var authors = book.TryGetProperty("authors", out var a)
                ? a.EnumerateArray().Select(x => x.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "").ToList()
                : new List<string>();
            var coverUrl = book.TryGetProperty("cover", out var cv) && cv.TryGetProperty("medium", out var m) ? m.GetString() : null;
            return new OpenLibraryBook(title, authors, null, coverUrl, isbn, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Open Library ISBN lookup failed: {Isbn}", isbn);
            return null;
        }
    }
}
