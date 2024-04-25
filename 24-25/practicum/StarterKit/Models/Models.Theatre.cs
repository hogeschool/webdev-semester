namespace StarterKit.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public required List<Reservation> Reservations { get; set; }
    }

    public class Reservation
    {
        public int ReservationId { get; set; }

        public int AmountOfTickets { get; set; }

        public bool Used { get; set; }

        public required Customer Customer { get; set; }

        public required TheatreShowDate TheatreShowDate { get; set; }
    }

    public class TheatreShowDate
    {
        public int TheatreShowDateId { get; set; }

        public DateTime DateAndTime { get; set; }

        public required List<Reservation> Reservations { get; set; }

        public required TheatreShow TheatreShow { get; set; }

    }

    public class TheatreShow
    {
        public int TheatreShowId { get; set; }
        public required string Title { get; set; }

        public required string Description { get; set; }

        public double Price { get; set; }

        public required List<TheatreShowDate> theatreShowDates { get; set; }

        public required Venue Venue { get; set; }

    }

    public class Venue
    {
        public int VenueId { get; set; }

        public required string Name { get; set; }

        public int Capacity { get; set; }

        public required List<TheatreShow> TheatreShows { get; set; }
    }
}