using System;
using Turista.Data.Properties;

namespace Turista.Data.Reservation
{
    public class PropertyCancellationPolicy : Entity
    {
        public Guid CancellationPolicyId { get; set; }
        public Guid PropertyId { get; set; }
        public Property Property { get; set; }
        public CancellationPolicy CancellationPolicy { get; set; }
    }
}
