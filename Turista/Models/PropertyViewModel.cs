using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Properties;

namespace Turista.Models
{
    public class PropertyViewModel
    {
        public List<Property> Properties { get; set; }
        public int PageNumber { get; set; }
        public int TotalRecord { get; set; }
        public int PageSize { get; set; }
    }
}
