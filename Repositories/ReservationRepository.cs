using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _db;
    public ReservationRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Reservation>> GetAllAsync() =>
        await _db.Reservations.Include(r => r.User).Include(r => r.Book).ToListAsync();

    public async Task<IEnumerable<Reservation>> GetByUserIdAsync(string userId) =>
        await _db.Reservations.Include(r => r.Book).Where(r => r.UserId == userId && r.IsActive).ToListAsync();

    public async Task<Reservation?> GetByIdAsync(int id) =>
        await _db.Reservations.Include(r => r.User).Include(r => r.Book).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Reservation> CreateAsync(Reservation reservation) { _db.Reservations.Add(reservation); await _db.SaveChangesAsync(); return reservation; }
    public async Task<Reservation> UpdateAsync(Reservation reservation) { _db.Reservations.Update(reservation); await _db.SaveChangesAsync(); return reservation; }
}
