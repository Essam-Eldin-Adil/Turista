using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Properties;

namespace Turista.Models
{
    public class ReviewViewModel
    {
        public List<Review> Reviews { get; set; }
        public int totalReviewCount { get; set; }
        public Property Property { get; set; }
    }


    public class PropertyForDept
    {
        public List<Property> Properties { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public bool hasMore { get; set; }
    }
}
