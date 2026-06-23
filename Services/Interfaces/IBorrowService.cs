using LibraryManagementSystem.DTOs;
namespace LibraryManagementSystem.Services.Interfaces;
public interface IBorrowService
{
    Task<IEnumerable<BorrowRecordResponse>> GetAllAsync();
    Task<IEnumerable<BorrowRecordResponse>> GetByUserIdAsync(string userId);
    Task<IEnumerable<BorrowRecordResponse>> GetOverdueAsync();
    Task<BorrowRecordResponse> IssueBookAsync(IssueBookRequest request);
    Task<BorrowRecordResponse> ReturnBookAsync(ReturnBookRequest request);
}
