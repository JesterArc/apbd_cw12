using WebApplication.Models.DTOs;

namespace WebApplication.Services;

public interface ITripService
{
    public Task<ICollection<TripDto>> GetTripsAsync();
    public Task<PageTripDto> GetTripsAsync(int page, int pageSize);
    public Task<ICollection<int>> GetTripIdsAsync();
}