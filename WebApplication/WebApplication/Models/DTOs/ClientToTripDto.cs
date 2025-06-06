namespace WebApplication.Models.DTOs;

public class ClientToTripDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Telephone { get; set; }
    public string Pesel { get; set; }
    // I don't really get why in the example this field exists when the tripId is already given in the Route
    public int IdTrip { get; set; }
    public string TripName { get; set; }
    public DateTime? PaymentDate { get; set; }
}