namespace LibraryManagementSystem.DTOs;

public record CreateReservationRequest(string UserId, int BookId);

public record ReservationResponse(
    int Id, string UserFullName, string BookTitle,
    DateTime ReservedAt, DateTime ExpiresAt, bool IsActive);
