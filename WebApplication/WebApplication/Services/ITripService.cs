using WebApplication.Models.DTOs;

namespace WebApplication.Services;

public interface ITripService
{
    public Task<ICollection<TripDto>> GetTripsAsync();
    public Task<ICollection<int>> GetTripIdsAsync();
}