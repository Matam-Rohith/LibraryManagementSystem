namespace LibraryManagementSystem.Models;

public class Fine
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int BorrowRecordId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; } = false;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public User User { get; set; } = null!;
    public BorrowRecord BorrowRecord { get; set; } = null!;
}
