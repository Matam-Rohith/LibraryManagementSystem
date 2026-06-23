namespace LibraryManagementSystem.DTOs;

public record MemberResponse(
    string Id, string FullName, string Email, string MembershipId,
    DateTime RegisteredAt, bool IsActive, int ActiveBorrows, decimal UnpaidFines);
