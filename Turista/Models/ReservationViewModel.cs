using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Identity;
using Turista.Data.Properties;
using Turista.Data.Reservation;

namespace Turista.Models
{
    public class ReservationsViewModel
    {
        public List<Reservation> Reservations { get; set; }
    }
    public class ReservationViewModel
    {
        public Reservation Reservation { get; set; }
        public User User { get; set; }
        public Property Property { get; set; }
        public List<OfferViewModel> offers { get; set; }
        public double DiscountFromOffers { get; set; }
        public double TotalReservationPrice { get; set; }
        public int TotalDays { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

}
