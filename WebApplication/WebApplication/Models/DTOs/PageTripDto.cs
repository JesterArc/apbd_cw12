namespace WebApplication.Models.DTOs;

public class PageTripDto
{
    public int PageNum { get; set; }
    public int PageSize { get; set; }
    public int AllPages { get; set; }
    public ICollection<TripDto> Trips { get; set; }
}