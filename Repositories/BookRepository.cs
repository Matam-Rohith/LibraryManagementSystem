using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _db;
    public BookRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Book>> GetAllAsync(string? search = null, string? category = null)
    {
        var q = _db.Books.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(b => b.Title.Contains(search) || b.Author.Contains(search) || b.ISBN.Contains(search));
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(b => b.Category == category);
        return await q.OrderBy(b => b.Title).ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id) =>
        await _db.Books.FindAsync(id);

    public async Task<Book?> GetByISBNAsync(string isbn) =>
        await _db.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);

    public async Task<Book> CreateAsync(Book book)
    {
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return book;
    }

    public async Task<Book> UpdateAsync(Book book)
    {
        _db.Books.Update(book);
        await _db.SaveChangesAsync();
        return book;
    }

    public async Task DeleteAsync(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is not null) { _db.Books.Remove(book); await _db.SaveChangesAsync(); }
    }
}
