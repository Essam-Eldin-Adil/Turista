using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Identity;
using Turista.Data.Properties;

namespace Turista.Data.Reservation
{
    public class Reservation : Entity
    {
        public int? ReservationNumber { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid PropertyId { get; set; }
        public Property Property { get; set; }
        public double DayPrice { get; set; }
        public double TotalPrice { get; set; }
        public int TotalDays { get; set; }
        public int Status { get; set; }
        public int ReservedBy { get; set; }
        public string ReservedByUser { get; set; }
        public string Description { get; set; }
        public string CancelResones { get; set; }
        public ICollection<PaymentTransaction> Invoices { get; set; }
        public double OfferDiscount { get; set; }
    }

    public class PaymentTransaction : Entity
    {
        public int PaymentType { get; set; }
        public DateTime PaymentDateTime { get; set; }
        public double Amount { get; set; }
        public Guid UserId { get; set; }
        public Guid ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        public User User { get; set; }
        public string RefNo { get; set; }
        public int InvoiceNumber { get; set; }
        public int Paidby { get; set; }
        public string PaidByUser { get; set; }
    }
}
