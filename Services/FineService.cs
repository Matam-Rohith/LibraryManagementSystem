using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services;

public class FineService : IFineService
{
    private readonly IFineRepository _repo;
    public FineService(IFineRepository repo) => _repo = repo;

    public async Task<IEnumerable<FineResponse>> GetAllAsync() =>
        (await _repo.GetAllAsync()).Select(Map);

    public async Task<IEnumerable<FineResponse>> GetByUserIdAsync(string userId) =>
        (await _repo.GetByUserIdAsync(userId)).Select(Map);

    public async Task<FineResponse> PayFineAsync(PayFineRequest req)
    {
        var fine = await _repo.GetByIdAsync(req.FineId)
            ?? throw new KeyNotFoundException("Fine not found.");

        if (fine.IsPaid)
            throw new InvalidOperationException("Fine already paid.");

        fine.IsPaid = true;
        fine.PaidAt = DateTime.UtcNow;
        return Map(await _repo.UpdateAsync(fine));
    }

    private static FineResponse Map(Fine f) =>
        new(f.Id, f.User?.FullName ?? f.UserId,
            f.BorrowRecord?.Book?.Title ?? "",
            f.Amount, f.IsPaid, f.IssuedAt, f.PaidAt);
}
