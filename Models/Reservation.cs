namespace LibraryManagementSystem.Models;

public class Reservation
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int BookId { get; set; }
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public User User { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
