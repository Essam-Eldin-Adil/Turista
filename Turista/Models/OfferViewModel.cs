using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Properties;

namespace Turista.Models
{
    public class OfferViewModel
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public bool HasOffer { get; set; } = false;
        public double OldAmount { get; set; }
        public Property Property { get; set; }
    }


}
