using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Turista.Data.Properties
{
    public class City : Entity
    {
        public string CityName { get; set; }
        public Guid? Image { get; set; }
        [NotMapped]
        public string ImageUrl { get; set; }
    }
}
