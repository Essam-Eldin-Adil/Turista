using System;
using System.ComponentModel.DataAnnotations;

namespace Turista.Data.Properties
{
    public class Offer : Entity
    {

        [DataType(DataType.Date)]
        public DateTime DateFrom { get; set; } = DateTime.Now;
        [DataType(DataType.Date)]
        public DateTime DateTo { get; set; } = DateTime.Now.AddDays(1);
        public double Amount { get; set; }
        public Property Property { get; set; }
        public Guid PropertyId { get; set; }
    }
}