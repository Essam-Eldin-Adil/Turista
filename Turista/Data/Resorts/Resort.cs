using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Properties;
using Turista.Data.Resorts.PropertyDetails;

namespace Turista.Data.Resorts
{
    public class Resort:Entity
    {
        public Resort()
        {
            ResortImages = new List<ResortImage>();
        }
        public string Name { get; set; }
        public bool ViewStatus { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public int Direction { get; set; }
        public Guid CityId { get; set; }
        public City City { get; set; }
        public string Region { get; set; }
        public string Neighborhood { get; set; }
        public bool CloseToSea { get; set; }
        public int DistanceFromSea { get; set; }
        public bool IsConfirmed { get; set; }
        public int CompleteStep { get; set; }
        public ICollection<ResortImage> ResortImages { get; set; }
        public ICollection<ResortUser> ResortUsers { get; set; }

        public ICollection<ResortParameterValue> ResortParameterValues { get; set; }
    }
}
