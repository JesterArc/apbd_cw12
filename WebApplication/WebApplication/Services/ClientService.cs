using Microsoft.EntityFrameworkCore;
using WebApplication.Data;

namespace WebApplication.Services;

public class ClientService : IClientService
{
    private readonly Apbd2Context _context;

    public ClientService(Apbd2Context context)
    {
        _context = context;
    }
    public async Task DeleteUserAsync(int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Clients.Where(c => c.IdClient == userId).ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DoesClientExistAsync(int userId)
    {
        return await _context.Clients.AnyAsync(c => c.IdClient == userId);
    }

    public async Task<bool> IsUserOnAnyTripsAsync(int userId)
    {
        return await _context.ClientTrips.AnyAsync(ct => ct.IdClient == userId);
    }
}