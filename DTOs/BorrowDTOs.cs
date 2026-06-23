namespace LibraryManagementSystem.DTOs;

public record IssueBookRequest(string UserId, int BookId, int DueDays = 14);

public record BorrowRecordResponse(
    int Id, string UserFullName, string BookTitle, string ISBN,
    DateTime BorrowedAt, DateTime DueDate, DateTime? ReturnedAt,
    bool IsReturned, bool IsOverdue);

public record ReturnBookRequest(int BorrowRecordId);
