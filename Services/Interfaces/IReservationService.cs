using LibraryManagementSystem.DTOs;
namespace LibraryManagementSystem.Services.Interfaces;
public interface IReservationService
{
    Task<IEnumerable<ReservationResponse>> GetAllAsync();
    Task<IEnumerable<ReservationResponse>> GetByUserIdAsync(string userId);
    Task<ReservationResponse> CreateAsync(CreateReservationRequest request);
    Task CancelAsync(int id);
}
