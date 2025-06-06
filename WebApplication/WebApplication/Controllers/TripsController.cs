using Microsoft.AspNetCore.Mvc;
using WebApplication.Data;
using WebApplication.Services;
using WebApplication.Models.DTOs;

namespace WebApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }
    [HttpGet]
    public async Task<IActionResult> GetTripsAsync(int page, int pageSize = 10)
    {
        switch (page)
        {
            case 0:
                return Ok(await _tripService.GetTripsAsync());
            case < 1:
                return BadRequest("Page number cannot be less than 1");
        }
        if (pageSize < 1)
        {
            return BadRequest("Page size cannot be less than 1");
        }
        var pageTrip = await _tripService.GetTripsAsync(page, pageSize);
        if (pageTrip.AllPages == 0)
        {
            return NoContent();
        }
        if (page > pageTrip.AllPages)
        {
            return NotFound($"Page number is out of range, Pages Available: 1-{pageTrip.AllPages}");
        }
        return Ok(pageTrip);
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AddClientToTrip([FromRoute] int idTrip, [FromBody] ClientToTripDto client)
    {
        // Swapped the order of the first two checks from this task because this edge case could never happen otherwise
        // if the client doesn't exist there won't be a trip associated with him
        // if he exists, the first check would prevent this from triggering, moving on...
        
        // Checks if client with this pesel is already signed up for trip with this id
        if (await _tripService.IsClientSingedUpForThisTripAsync(client.Pesel, idTrip))
        {
            return BadRequest($"Client with Pesel {client.Pesel} is already signed up for trip with id = {idTrip}");
        }
        // Check if client with this pesel exists
        if (await _tripService.DoesClientExistAsync(client.Pesel))
        {
            return BadRequest($"Client with Pesel {client.Pesel} already exists");
        }

        if (idTrip != client.IdTrip)
        {
            return BadRequest($"Inconsistent trip ID: {idTrip} / {client.IdTrip}");
        }
        // Checks if this trip exists
        if (!await _tripService.DoesTripExistAsync(idTrip, client.TripName))
        {
            return NotFound($"No trip with id {idTrip} and name {client.TripName} exists");
        }
        // Checks if trip has already ended
        if (await _tripService.HasTheTripAlreadyHappenedAsync(idTrip))
        {
            return BadRequest("Cannot sign up for a trip that has already started");
        }

        try
        {
            // returns info about newly created client
            return Ok(await _tripService.AddClientToTripAsync(client));
        }
        catch (Exception)
        {
            return BadRequest("Transaction failed");
        }
    }
}