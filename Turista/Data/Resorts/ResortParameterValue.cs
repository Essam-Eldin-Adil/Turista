using System;
using Turista.Data.Properties.PropertyDetails;

namespace Turista.Data.Resorts.PropertyDetails
{
    public class ResortParameterValue : Entity
    {
        public Parameter Parameter { get; set; }
        public Guid ParameterId { get; set; }
        public Resort Resort { get; set; }
        public Guid ResortId { get; set; }
        public string Value { get; set; }
    }
}
