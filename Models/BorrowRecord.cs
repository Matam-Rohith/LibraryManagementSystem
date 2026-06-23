namespace LibraryManagementSystem.Models;

public class BorrowRecord
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int BookId { get; set; }
    public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public bool IsReturned { get; set; } = false;
    public User User { get; set; } = null!;
    public Book Book { get; set; } = null!;
    public Fine? Fine { get; set; }
}
