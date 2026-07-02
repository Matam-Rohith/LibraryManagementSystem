namespace LibraryManagementSystem.Services.Interfaces;

public record OpenLibraryBook(
    string Title,
    List<string> Authors,
    int? FirstPublishYear,
    string? CoverUrl,
    string? Isbn,
    string? Subject);

public interface IOpenLibraryService
{
    Task<List<OpenLibraryBook>> SearchBooksAsync(string query, int limit = 10);
    Task<OpenLibraryBook?> GetBookByIsbnAsync(string isbn);
}
