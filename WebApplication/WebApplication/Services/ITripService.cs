using WebApplication.Models.DTOs;

namespace WebApplication.Services;

public interface ITripService
{
    public Task<ICollection<TripDto>> GetTripsAsync();
    public Task<PageTripDto> GetTripsAsync(int page, int pageSize);
    public Task<bool> DoesClientExistAsync(string pesel);
    public Task<bool> IsClientSingedUpForThisTripAsync(string pesel, int tripId);
    public Task<bool> DoesTripExistAsync(int tripId, string tripName);
    public Task<bool> HasTheTripAlreadyHappenedAsync(int tripId);
    public Task<ClientWithIdDto> AddClientToTripAsync(ClientToTripDto clientDto);
}