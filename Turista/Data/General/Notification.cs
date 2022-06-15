using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Identity;

namespace Turista.Data.General
{
    public class Notification:Entity
    {
        public User User { get; set; }
        public Guid UserId { get; set; }
        public int Type { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsViewed { get; set; } = false;
        public string URL { get; set; }

    }
}
