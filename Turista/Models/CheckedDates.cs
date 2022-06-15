using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Properties;

namespace Turista.Models
{
    public class CheckedDates
    {
        public CheckedDates()
        {
            CheckIn = DateTime.Now;
            CheckOut = DateTime.Now.AddDays(1);
            DaysCount = 1;
        }
        public int DaysCount { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public Property Property { get; set; }
    }
}
