using System;
using Turista.Data.Identity;

namespace Turista.Data.Resorts
{
    public class ResortUser : Entity
    {
        public bool SendWhatsAppNotifications { get; set; }
        public bool IsAdmin { get; set; }
        public Guid ResortId { get; set; }
        public Guid UserId { get; set; }
        public Resort Resort { get; set; }
        public User User { get; set; }
    }
}
