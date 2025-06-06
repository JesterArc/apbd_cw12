using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
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
        var tripIds = await GetTripIdsAsync();
        var clientList  = new Dictionary<int, List<ClientDto>>();
        var countriesList = new Dictionary<int, List<CountryDto>>();
        foreach (var tripId in tripIds)
        {
            clientList.Add(tripId, await _context.Clients
                .Join(_context.ClientTrips, c => c.IdClient, ct => ct.IdClient, (c, ct) => new {c, ct})
                .Where(cct => cct.ct.IdTrip == tripId)
                .Select(cct => new ClientDto() {FirstName = cct.c.FirstName, LastName = cct.c.LastName})
                .ToListAsync());
            countriesList.Add(tripId, await _context.Countries
                .Where(c => c.IdTrips.Any(sub => sub.IdTrip.Equals(tripId)))
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
            .OrderByDescending(t => t.DateFrom)
            .ToListAsync();
    }

    public async Task<PageTripDto> GetTripsAsync(int page, int pageSize)
    {
        var trips = await GetTripsAsync();
        return new PageTripDto()
        {
            PageNum = page,
            PageSize = pageSize,
            AllPages = (int) Math.Ceiling(trips.Count * 1.0 / pageSize),
            Trips = trips.Skip((page - 1) * pageSize).Take(pageSize).ToList()
        };
    }

    public async Task<ICollection<int>> GetTripIdsAsync()
    {
        return await _context.Trips.Select(t => t.IdTrip).ToListAsync();
    }
}