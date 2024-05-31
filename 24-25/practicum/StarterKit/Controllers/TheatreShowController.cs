using System.Text;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using StarterKit.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace StarterKit.Controllers;


[Route("api/v1/TheatreShow")]
public class TheatreShowController : Controller
{

    private readonly DatabaseContext _context;
    public TheatreShowController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] TheatreShowRequestBody theatreShow)
    {
        if (theatreShow.TheatreShowDates != null && theatreShow.Venue != null)
        {
            _context.TheatreShow.Add(new TheatreShow
            {
                Title = theatreShow.Title,
                Description = theatreShow.Description,
                Price = theatreShow.Price,
                theatreShowDates = theatreShow.TheatreShowDates.Select(v => new TheatreShowDate { DateAndTime = DateTime.ParseExact(v.DateAndTime, "MM-dd-yyyy HH:mm", CultureInfo.InvariantCulture) }).ToList(),
                Venue = new Venue
                {
                    Name = theatreShow.Venue.Name,
                    Capacity = theatreShow.Venue.Capacity
                }
            });
            _context.SaveChanges();
            return Ok("Added new show");
        }
        return BadRequest("JSON is incomplete");
    }

    [HttpGet("Get/{id?}")]
    public IActionResult Get(int? id)
    {
        if (id == null)
        {
            var entries = (from show in _context.TheatreShow 
            join venue in _context.Venue on show.Venue!.VenueId equals venue.VenueId
            select new {
                Title = show.Title, 
                Description = show.Description, 
                Price = show.Price,
                Venue = venue.Name,
                Capacity = venue.Capacity
            }

            );
            return Ok(entries.ToArray());
        }
        var entry = _context.TheatreShow.FirstOrDefault(v => v.TheatreShowId == id);
        if (entry == null) return NotFound("Entity not found");
        return Ok(entry);
    }

    

}
public partial class TheatreShowRequestBody
{
    [JsonPropertyName("Title")]
    public string? Title { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Price")]
    public double Price { get; set; }

    [JsonPropertyName("theatreShowDates")]
    public TheatreShowDateRequestBody[]? TheatreShowDates { get; set; }

    [JsonPropertyName("Venue")]
    public VenueRequestBody? Venue { get; set; }
}

public partial class TheatreShowDateRequestBody
{
    [JsonPropertyName("DateAndTime")]
    public string? DateAndTime { get; set; }
}

public partial class VenueRequestBody
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Capacity")]
    public int Capacity { get; set; }
}
