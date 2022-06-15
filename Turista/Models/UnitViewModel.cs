using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Properties;
using Turista.Data.Properties.PropertyDetails;

namespace Turista.Models
{
    public class UnitViewModel
    {
        public UnitViewModel()
        {
            Review = new Review();
        }
        public Property Property { get; set; }
        public List<ParameterGroup> ParameterGroups { get; set; }
        public Review Review { get; set; }
    }

    public class SearchViewModel
    {
        public List<Guid> City { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<int> ProprtyType { get; set; }
        public List<ParameterGroup> ParameterGroups { get; set; }
        public List<City> Cities { get; set; }
    }
}
