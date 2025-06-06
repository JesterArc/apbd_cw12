using Microsoft.AspNetCore.Mvc;
using WebApplication.Services;

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
}