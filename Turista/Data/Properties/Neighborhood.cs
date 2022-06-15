using System;

namespace Turista.Data.Properties
{
    public class Neighborhood : Entity
    {
        public Guid RegionId { get; set; }
        public Region Region { get; set; }
        public string Name { get; set; }
    }
}
