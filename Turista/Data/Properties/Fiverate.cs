using System;
using Turista.Data.Identity;

namespace Turista.Data.Properties
{
    public class Fiverate : Entity
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid PropertyId { get; set; }
        public Property Property { get; set; }
    }
}
