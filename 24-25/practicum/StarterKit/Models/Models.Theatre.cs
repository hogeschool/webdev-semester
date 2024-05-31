using Newtonsoft.Json;

namespace StarterKit.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public List<Reservation>? Reservations { get; set; }
    }

    public class Reservation
    {
        public int ReservationId { get; set; }

        public int AmountOfTickets { get; set; }

        public bool Used { get; set; }

        public Customer? Customer { get; set; }

        public TheatreShowDate? TheatreShowDate { get; set; }
    }

    public class TheatreShowDate
    {
        public int TheatreShowDateId { get; set; }

        public DateTime DateAndTime { get; set; } //"MM-dd-yyyy HH:mm"

        public List<Reservation>? Reservations { get; set; }

        public TheatreShow? TheatreShow { get; set; }

    }

    public class TheatreShow
    {
        public int TheatreShowId { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public double Price { get; set; }

        public List<TheatreShowDate>? theatreShowDates { get; set; }

        public Venue? Venue { get; set; }

    }

    public class Venue
    {
        public int VenueId { get; set; }

        public string? Name { get; set; }

        public int Capacity { get; set; }

        public List<TheatreShow>? TheatreShows { get; set; }
    }
}