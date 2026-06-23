using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryManagementSystem.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _repo;
    public BookService(IBookRepository repo) => _repo = repo;

    public async Task<IEnumerable<BookResponse>> GetAllAsync(string? search, string? category) =>
        (await _repo.GetAllAsync(search, category)).Select(Map);

    public async Task<BookResponse> GetByIdAsync(int id) =>
        Map(await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Book not found."));

    public async Task<BookResponse> CreateAsync(CreateBookRequest req)
    {
        if (await _repo.GetByISBNAsync(req.ISBN) != null)
            throw new InvalidOperationException("A book with this ISBN already exists.");

        var book = new Book
        {
            Title = req.Title, Author = req.Author, ISBN = req.ISBN,
            Category = req.Category, Publisher = req.Publisher,
            PublishedYear = req.PublishedYear, TotalCopies = req.TotalCopies,
            AvailableCopies = req.TotalCopies
        };
        return Map(await _repo.CreateAsync(book));
    }

    public async Task<BookResponse> UpdateAsync(int id, UpdateBookRequest req)
    {
        var book = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Book not found.");
        var diff = req.TotalCopies - book.TotalCopies;
        book.Title = req.Title; book.Author = req.Author; book.Category = req.Category;
        book.Publisher = req.Publisher; book.PublishedYear = req.PublishedYear;
        book.TotalCopies = req.TotalCopies;
        book.AvailableCopies = Math.Max(0, book.AvailableCopies + diff);
        return Map(await _repo.UpdateAsync(book));
    }

    public async Task DeleteAsync(int id) => await _repo.DeleteAsync(id);

    private static BookResponse Map(Book b) =>
        new(b.Id, b.Title, b.Author, b.ISBN, b.Category, b.Publisher, b.PublishedYear, b.TotalCopies, b.AvailableCopies);
}
