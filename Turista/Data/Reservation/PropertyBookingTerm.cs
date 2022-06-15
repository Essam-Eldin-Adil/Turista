using System;
using Turista.Data.Properties;

namespace Turista.Data.Reservation
{
    public class PropertyBookingTerm : Entity
    {
        public Guid BookingTermId { get; set; }
        public Guid PropertyId { get; set; }
        public Property Property { get; set; }
        public BookingTerm BookingTerm { get; set; }
    }
}
