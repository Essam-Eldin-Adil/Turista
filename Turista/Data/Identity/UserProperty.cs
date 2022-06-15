using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.General;
using Turista.Data.Properties;

namespace Turista.Data.Identity
{
    public class UserProperty : Entity
    {
        public User User { get; set; }
        public Guid UserId { get; set; }
        public Property Properties { get; set; }
        public Guid PropertyId { get; set; }
    }
}
