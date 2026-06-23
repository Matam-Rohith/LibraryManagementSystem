using LibraryManagementSystem.DTOs;
namespace LibraryManagementSystem.Services.Interfaces;
public interface IBookService
{
    Task<IEnumerable<BookResponse>> GetAllAsync(string? search, string? category);
    Task<BookResponse> GetByIdAsync(int id);
    Task<BookResponse> CreateAsync(CreateBookRequest request);
    Task<BookResponse> UpdateAsync(int id, UpdateBookRequest request);
    Task DeleteAsync(int id);
}
