using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Identity;

namespace Turista.Data.Properties
{
    public class Review:Entity
    {
        public double Cleaning { get; set; }
        public double Crew { get; set; }
        public double PropertyCondition { get; set; }
        public string Comment { get; set; }
        public bool IsReplay { get; set; }
        public Guid? ReplayedToId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid PropertyId { get; set; }
        public Property Property { get; set; }
        [NotMapped]
        public virtual List<Review> Replyeds { get; set; }
    }
}
