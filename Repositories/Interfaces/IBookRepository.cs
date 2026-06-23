using LibraryManagementSystem.Models;
namespace LibraryManagementSystem.Repositories.Interfaces;
public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync(string? search = null, string? category = null);
    Task<Book?> GetByIdAsync(int id);
    Task<Book?> GetByISBNAsync(string isbn);
    Task<Book> CreateAsync(Book book);
    Task<Book> UpdateAsync(Book book);
    Task DeleteAsync(int id);
}
