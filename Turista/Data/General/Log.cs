using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Turista.Data.General
{
    public class Log:Entity
    {
        public string Message { get; set; }
        public string Content { get; set; }
        public int Type { get; set; }
        public bool IsResolved { get; set; } = false;
        public Guid UserId { get; set; } = Guid.Empty;
    }


}
