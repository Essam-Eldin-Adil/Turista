using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Turista.Data.Identity
{
    public class RolePermission:Entity
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public virtual Role Role { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
