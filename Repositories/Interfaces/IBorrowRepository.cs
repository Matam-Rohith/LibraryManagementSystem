using LibraryManagementSystem.Models;
namespace LibraryManagementSystem.Repositories.Interfaces;
public interface IBorrowRepository
{
    Task<IEnumerable<BorrowRecord>> GetAllAsync();
    Task<IEnumerable<BorrowRecord>> GetByUserIdAsync(string userId);
    Task<IEnumerable<BorrowRecord>> GetOverdueAsync();
    Task<BorrowRecord?> GetByIdAsync(int id);
    Task<BorrowRecord> CreateAsync(BorrowRecord record);
    Task<BorrowRecord> UpdateAsync(BorrowRecord record);
}
