using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Properties;
using Turista.Data.Properties.PropertyDetails;
using Turista.Data.Resorts;
using Turista.Data.Resorts.PropertyDetails;

namespace Turista.Models
{
    public class ResortViewModel
    {
        public ResortViewModel()
        {
            ResortParameterValues = new List<ResortParameterValue>();
        }
        public Resort Resort { get; set; }
        public List<ResortParameterValue> ResortParameterValues { get; set; }
        public List<ParameterGroup> ParameterGroups { get; set; }
    }

    public class PropertyParameterViewModel
    {
        public PropertyParameterViewModel()
        {
            ParameterValues = new List<ParameterValue>();
        }
        public Property Property { get; set; }
        public List<ParameterValue> ParameterValues { get; set; }
        public List<ParameterGroup> ParameterGroups { get; set; }
    }
}
