using Microsoft.AspNetCore.Mvc;
using WebApplication.Services;

namespace WebApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClientAsync(int id)
    {
        if (!await _clientService.DoesClientExistAsync(id))
        {
            return NotFound($"No client found with id = {id}");
        }

        if (await _clientService.IsUserOnAnyTripsAsync(id))
        {
            return BadRequest($"User with id = {id} is assigned on a trip. You cannot remove assigned clients.");
        }

        try
        {
            await _clientService.DeleteUserAsync(id);
            return Ok($"Client with id = {id} deleted.");
        }
        catch (Exception)
        {
            return BadRequest("Transaction failed");
        }
    }
}