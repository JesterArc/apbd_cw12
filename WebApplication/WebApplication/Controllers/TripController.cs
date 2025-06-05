using Microsoft.AspNetCore.Mvc;
using WebApplication.Services;

namespace WebApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpGet()]
    public async Task<IActionResult> GetTrips(int id)
    {
        var trips = await _tripService.GetTripsAsync();
        return Ok(trips);
    }
}