using System.Text;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using StarterKit.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
            return Ok("Entry created");
        }
        return BadRequest("JSON is incomplete");
    }

    [HttpGet("Get/{id?}")]
    public IActionResult Get(int? id, string? search, string? location, string? from_date, string? to_date, string? sort)
    {
        var date_entries = from dates in _context.TheatreShowDate
                           join show in _context.TheatreShow on dates.TheatreShow.TheatreShowId equals show.TheatreShowId
                           select new
                           {
                               dates.TheatreShowDateId,
                               dates.DateAndTime,
                               show.TheatreShowId
                           };

        var entries = from show in _context.TheatreShow
                      join venue in _context.Venue on show.Venue!.VenueId equals venue.VenueId
                      select new
                      {
                          show.TheatreShowId,
                          show.Title,
                          show.Description,
                          show.Price,
                          VenueName = venue.Name,
                          venue.Capacity,
                          dates = date_entries.Where(d => d.TheatreShowId == show.TheatreShowId).ToArray()
                      };

        if (id != null)
        {
            var single_result = entries.Where(e => e.TheatreShowId == id).ToArray().FirstOrDefault();
            if (single_result == null) return NotFound("Entry not found");
            return Ok(single_result);
        }

        if (search != null) {
            entries = entries.Where(e => e.Title.Contains(search) || e.Description.Contains(search));
        }

        if (location != null) {
            entries = entries.Where(e => e.VenueName == location);
        }

        if (from_date != null || to_date != null) {

        }

        if (sort != null) {
            
        }

        return Ok(entries.ToArray().DistinctBy(v => v.TheatreShowId));
    }

    [HttpPut("Update")]
    public IActionResult Update([FromBody] TheatreShowRequestBody theatreShow)
    {
        _context.TheatreShow.Update(new TheatreShow
        {
            TheatreShowId = theatreShow.TheatreShowId,
            Title = theatreShow.Title,
            Description = theatreShow.Description,
            Price = theatreShow.Price,
            theatreShowDates = theatreShow.TheatreShowDates.Select(v => new TheatreShowDate
            {
                TheatreShowDateId = v.TheatreShowDateId,
                DateAndTime = DateTime.Parse(v.DateAndTime)
            }).ToList(),
            Venue = new Venue
            {
                VenueId = theatreShow.Venue.VenueId,
                Name = theatreShow.Venue.Name,
                Capacity = theatreShow.Venue.Capacity
            }
        });
        _context.SaveChanges();
        return Ok(theatreShow);
    }

    [HttpDelete("Delete/{id?}")]
    public IActionResult Delete(int id)
    {
        var entry = _context.TheatreShow.Where(v => v.TheatreShowId == id).FirstOrDefault();
        if (entry == null) return NotFound();
        _context.TheatreShow.Remove(entry);
        _context.SaveChanges();
        return Ok("Deleted entry " + id);
    }




}
public partial class TheatreShowRequestBody
{
    [JsonPropertyName("TheatreShowId")]
    public int TheatreShowId { get; set; }

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
    [JsonPropertyName("TheatreShowDateId")]
    public int TheatreShowDateId { get; set; }

    [JsonPropertyName("DateAndTime")]
    public string? DateAndTime { get; set; }
}

public partial class VenueRequestBody
{
    [JsonPropertyName("VenueId")]
    public int VenueId { get; set; }
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Capacity")]
    public int Capacity { get; set; }
}
