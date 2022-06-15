using Turista.Data.Properties;

namespace Turista.Models
{
    public class CityViewModel
    {
        public CityViewModel()
        {
            City = new City();
        }
        public City City { get; set; }
    }
}
