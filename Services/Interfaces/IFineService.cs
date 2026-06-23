using LibraryManagementSystem.DTOs;
namespace LibraryManagementSystem.Services.Interfaces;
public interface IFineService
{
    Task<IEnumerable<FineResponse>> GetAllAsync();
    Task<IEnumerable<FineResponse>> GetByUserIdAsync(string userId);
    Task<FineResponse> PayFineAsync(PayFineRequest request);
}
