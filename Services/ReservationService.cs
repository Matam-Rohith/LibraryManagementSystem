using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryManagementSystem.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _repo;
    private readonly IBookRepository _bookRepo;
    public ReservationService(IReservationRepository repo, IBookRepository bookRepo)
    {
        _repo = repo; _bookRepo = bookRepo;
    }

    public async Task<IEnumerable<ReservationResponse>> GetAllAsync() =>
        (await _repo.GetAllAsync()).Select(Map);

    public async Task<IEnumerable<ReservationResponse>> GetByUserIdAsync(string userId) =>
        (await _repo.GetByUserIdAsync(userId)).Select(Map);

    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest req)
    {
        var book = await _bookRepo.GetByIdAsync(req.BookId)
            ?? throw new KeyNotFoundException("Book not found.");

        var reservation = new Reservation
        {
            UserId = req.UserId,
            BookId = req.BookId,
            ExpiresAt = DateTime.UtcNow.AddDays(3)
        };
        return Map(await _repo.CreateAsync(reservation));
    }

    public async Task CancelAsync(int id)
    {
        var reservation = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Reservation not found.");
        reservation.IsActive = false;
        await _repo.UpdateAsync(reservation);
    }

    private static ReservationResponse Map(Reservation r) =>
        new(r.Id, r.User?.FullName ?? r.UserId, r.Book?.Title ?? "",
            r.ReservedAt, r.ExpiresAt, r.IsActive);
}
