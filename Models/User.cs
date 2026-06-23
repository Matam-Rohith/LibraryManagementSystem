using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Models;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string MembershipId { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    public ICollection<Fine> Fines { get; set; } = new List<Fine>();
}
