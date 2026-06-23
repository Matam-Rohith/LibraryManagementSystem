namespace LibraryManagementSystem.DTOs;

public record FineResponse(
    int Id, string UserFullName, string BookTitle,
    decimal Amount, bool IsPaid, DateTime IssuedAt, DateTime? PaidAt);

public record PayFineRequest(int FineId);
