using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories;

public class BorrowRepository : IBorrowRepository
{
    private readonly AppDbContext _db;
    public BorrowRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<BorrowRecord>> GetAllAsync() =>
        await _db.BorrowRecords.Include(b => b.User).Include(b => b.Book).OrderByDescending(b => b.BorrowedAt).ToListAsync();

    public async Task<IEnumerable<BorrowRecord>> GetByUserIdAsync(string userId) =>
        await _db.BorrowRecords.Include(b => b.Book).Where(b => b.UserId == userId).ToListAsync();

    public async Task<IEnumerable<BorrowRecord>> GetOverdueAsync() =>
        await _db.BorrowRecords.Include(b => b.User).Include(b => b.Book)
            .Where(b => !b.IsReturned && b.DueDate < DateTime.UtcNow).ToListAsync();

    public async Task<BorrowRecord?> GetByIdAsync(int id) =>
        await _db.BorrowRecords.Include(b => b.User).Include(b => b.Book).FirstOrDefaultAsync(b => b.Id == id);

    public async Task<BorrowRecord> CreateAsync(BorrowRecord record)
    {
        _db.BorrowRecords.Add(record);
        await _db.SaveChangesAsync();
        return record;
    }

    public async Task<BorrowRecord> UpdateAsync(BorrowRecord record)
    {
        _db.BorrowRecords.Update(record);
        await _db.SaveChangesAsync();
        return record;
    }
}
