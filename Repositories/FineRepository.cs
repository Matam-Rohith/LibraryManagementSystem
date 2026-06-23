using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories;

public class FineRepository : IFineRepository
{
    private readonly AppDbContext _db;
    public FineRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Fine>> GetAllAsync() =>
        await _db.Fines.Include(f => f.User).Include(f => f.BorrowRecord).ThenInclude(br => br.Book).ToListAsync();

    public async Task<IEnumerable<Fine>> GetByUserIdAsync(string userId) =>
        await _db.Fines.Include(f => f.BorrowRecord).ThenInclude(br => br.Book).Where(f => f.UserId == userId).ToListAsync();

    public async Task<Fine?> GetByIdAsync(int id) =>
        await _db.Fines.Include(f => f.User).Include(f => f.BorrowRecord).ThenInclude(br => br.Book).FirstOrDefaultAsync(f => f.Id == id);

    public async Task<Fine> CreateAsync(Fine fine) { _db.Fines.Add(fine); await _db.SaveChangesAsync(); return fine; }
    public async Task<Fine> UpdateAsync(Fine fine) { _db.Fines.Update(fine); await _db.SaveChangesAsync(); return fine; }
}
