using System;

namespace Turista.Data.Properties
{
    public class Region:Entity
    {
        public string Name { get; set; }
        public Guid CityId { get; set; }
        public City City { get; set; }
    }
}