using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Turista.Data.Properties.PropertyDetails
{
    public class ParameterValue : Entity
    {
        public Parameter Parameter { get; set; }
        public Guid ParameterId { get; set; }
        public Property Property { get; set; }
        public Guid PropertyId { get; set; }
        public string Value { get; set; }
    }
}
