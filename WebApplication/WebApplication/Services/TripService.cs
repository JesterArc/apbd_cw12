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

    public async Task<bool> DoesClientExistAsync(string pesel)
    {
        return await _context.Clients.AnyAsync(c => c.Pesel.Equals(pesel));
    }

    public async Task<bool> IsClientSingedUpForThisTripAsync(string pesel, int tripId)
    {
        return await _context.ClientTrips
            .Join(_context.Clients, ct => ct.IdClient, c => c.IdClient, (ct, c) => new {ct, c})
            .AnyAsync(ctc => ctc.c.Pesel == pesel && ctc.ct.IdTrip == tripId);
    }

    public async Task<bool> DoesTripExistAsync(int tripId, string tripName)
    {
        return await _context.Trips.AnyAsync(t => t.IdTrip == tripId && t.Name.Equals(tripName));
    }

    public async Task<bool> HasTheTripAlreadyHappenedAsync(int tripId)
    {
        return await _context.Trips.Where(t => t.IdTrip == tripId).Select(t => t.DateFrom).Where(dt => dt <= DateTime.Now).AnyAsync();
    }

    public async Task<ClientWithIdDto> AddClientToTripAsync(ClientToTripDto clientDto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Clients.AddAsync(new Client()
            {
                FirstName = clientDto.FirstName,
                LastName = clientDto.LastName,
                Pesel = clientDto.Pesel,
                Telephone = clientDto.Telephone,
                Email = clientDto.Email,
                ClientTrips = new List<ClientTrip>()
            });
            await _context.SaveChangesAsync();
            var id = await _context.Clients.Where(c => c.Pesel == clientDto.Pesel).Select(c => c.IdClient).FirstAsync();
            await _context.ClientTrips.AddAsync(new ClientTrip()
            {
                IdClient = id,
                IdTrip = clientDto.IdTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = clientDto.PaymentDate
            });
            await _context.SaveChangesAsync();
            var clientTrip = await _context.ClientTrips.Where(ct => ct.IdClient == id && ct.IdTrip == clientDto.IdTrip)
                .FirstAsync();
            var client = await _context.Clients.FindAsync(id);
            client.ClientTrips.Add(clientTrip);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return await _context.Clients.Where(c => c.IdClient == id).Select(c => new ClientWithIdDto()
            {
                IdClient = c.IdClient,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Pesel = c.Pesel,
                Telephone = c.Telephone,
                Email = c.Email
            }).FirstAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}