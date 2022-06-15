using System;
using Turista.Data.Identity;

namespace Turista.Data.Properties
{
    public class PropertyUser : Entity
    {
        public bool SendWhatsAppNotifications { get; set; }
        public bool IsAdmin { get; set; }
        public Guid PropertyId { get; set; }
        public Guid UserId { get; set; }
        public Property Property { get; set; }
        public User User { get; set; }
    }
}
