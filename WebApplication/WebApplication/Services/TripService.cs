using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Models;
using WebApplication.Models.DTOs;

namespace WebApplication.Services;

public class TripService : ITripService
{
    private readonly Apbd2Context _context;
    public TripService(Apbd2Context context)
    {
        _context = context;
    }
    
    public async Task<ICollection<TripDto>> GetTripsAsync()
    {
        var ids = await GetTripIdsAsync();
        Dictionary<int, List<ClientDto>> clientList  = new Dictionary<int, List<ClientDto>>();
        Dictionary<int, List<CountryDto>> countriesList = new Dictionary<int, List<CountryDto>>();
        foreach (var id in ids)
        {
            clientList.Add(id, await _context.Clients
                .Join(_context.ClientTrips, c => c.IdClient, ct => ct.IdClient, (c, ct) => new {c, ct})
                .Where(cct => cct.ct.IdTrip == id)
                .Select(cct => new ClientDto() {FirstName = cct.c.FirstName, LastName = cct.c.LastName})
                .ToListAsync());
            countriesList.Add(id, await _context.Countries
                .Where(c => c.IdTrips.Any(sub => sub.IdTrip.Equals(id)))
                .Select(c => new CountryDto(){Name = c.Name})
                .ToListAsync());
        }
        
        return await _context.Trips.Select(t => new TripDto()
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = countriesList[t.IdTrip],
                Clients = clientList[t.IdTrip]
            }
        )
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<ICollection<int>> GetTripIdsAsync()
    {
        return await _context.Trips.Select(t => t.IdTrip).ToListAsync();
    }
}