using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Turista.Data.Identity
{
    public class UserPermission:Entity
    {
        public Guid UserId { get; set; }
        public Guid PermissionId { get; set; }
        public virtual User User { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
