using System.Collections.Generic;
using Turista.Data.Properties.PropertyDetails;
using Turista.Data.Resorts;

namespace Turista.Models
{
    public class SingleResortViewModel
    {
        public Resort Resort { get; set; }
        public List<ParameterGroup> ParameterGroups { get; set; }
    }
}
