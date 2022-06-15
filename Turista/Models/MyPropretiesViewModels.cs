using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Properties;
using Turista.Data.Resorts;

namespace Turista.Models
{
    public class MyPropretiesViewModels
    {
        public List<Property> Properties { get; set; }
        public List<Resort> Resorts { get; set; }
    }
}
