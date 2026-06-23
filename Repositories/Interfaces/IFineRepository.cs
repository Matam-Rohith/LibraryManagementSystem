using LibraryManagementSystem.Models;
namespace LibraryManagementSystem.Repositories.Interfaces;
public interface IFineRepository
{
    Task<IEnumerable<Fine>> GetAllAsync();
    Task<IEnumerable<Fine>> GetByUserIdAsync(string userId);
    Task<Fine?> GetByIdAsync(int id);
    Task<Fine> CreateAsync(Fine fine);
    Task<Fine> UpdateAsync(Fine fine);
}
