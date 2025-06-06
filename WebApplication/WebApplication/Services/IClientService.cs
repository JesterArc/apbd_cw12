using WebApplication.Models;

namespace WebApplication.Services;

public interface IClientService
{
    public Task DeleteUserAsync(int userId);
    public Task<bool> DoesClientExistAsync(int userId);
    public Task<bool> IsUserOnAnyTripsAsync(int userId);
}