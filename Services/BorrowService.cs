using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryManagementSystem.Services;

public class BorrowService : IBorrowService
{
    private readonly IBorrowRepository _borrowRepo;
    private readonly IBookRepository _bookRepo;
    private readonly IFineRepository _fineRepo;

    public BorrowService(IBorrowRepository borrowRepo, IBookRepository bookRepo, IFineRepository fineRepo)
    {
        _borrowRepo = borrowRepo;
        _bookRepo = bookRepo;
        _fineRepo = fineRepo;
    }

    public async Task<IEnumerable<BorrowRecordResponse>> GetAllAsync() =>
        (await _borrowRepo.GetAllAsync()).Select(Map);

    public async Task<IEnumerable<BorrowRecordResponse>> GetByUserIdAsync(string userId) =>
        (await _borrowRepo.GetByUserIdAsync(userId)).Select(Map);

    public async Task<IEnumerable<BorrowRecordResponse>> GetOverdueAsync() =>
        (await _borrowRepo.GetOverdueAsync()).Select(Map);

    public async Task<BorrowRecordResponse> IssueBookAsync(IssueBookRequest req)
    {
        var book = await _bookRepo.GetByIdAsync(req.BookId)
            ?? throw new KeyNotFoundException("Book not found.");

        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException("No copies available for this book.");

        book.AvailableCopies--;
        await _bookRepo.UpdateAsync(book);

        var record = new BorrowRecord
        {
            UserId = req.UserId,
            BookId = req.BookId,
            DueDate = DateTime.UtcNow.AddDays(req.DueDays)
        };

        return Map(await _borrowRepo.CreateAsync(record));
    }

    public async Task<BorrowRecordResponse> ReturnBookAsync(ReturnBookRequest req)
    {
        var record = await _borrowRepo.GetByIdAsync(req.BorrowRecordId)
            ?? throw new KeyNotFoundException("Borrow record not found.");

        if (record.IsReturned)
            throw new InvalidOperationException("Book already returned.");

        record.IsReturned = true;
        record.ReturnedAt = DateTime.UtcNow;

        if (record.ReturnedAt > record.DueDate)
        {
            var days = (record.ReturnedAt.Value - record.DueDate).Days;
            await _fineRepo.CreateAsync(new Fine
            {
                UserId = record.UserId,
                BorrowRecordId = record.Id,
                Amount = days * 10m
            });
        }

        var book = await _bookRepo.GetByIdAsync(record.BookId);
        if (book != null) { book.AvailableCopies++; await _bookRepo.UpdateAsync(book); }

        return Map(await _borrowRepo.UpdateAsync(record));
    }

    private static BorrowRecordResponse Map(BorrowRecord b) =>
        new(b.Id, b.User?.FullName ?? b.UserId, b.Book?.Title ?? b.BookId.ToString(),
            b.Book?.ISBN ?? "", b.BorrowedAt, b.DueDate, b.ReturnedAt, b.IsReturned,
            !b.IsReturned && b.DueDate < DateTime.UtcNow);
}
