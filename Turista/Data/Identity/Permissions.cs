using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Turista.Data.Identity
{
    public class Page:Entity
    {
        public string Name { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public virtual ICollection<Permission> Permissions { get; set; }
    }
    public class Permission:Entity
    {
        public string Name { get; set; }
        public Guid PageId { get; set; }
        public virtual Page Page { get; set; }
        [NotMapped]
        public string PageName { get; set; }
    }

}
