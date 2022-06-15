using Turista.Resources;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Turista.Data.General;

namespace Turista.Data.Identity
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [NotMapped]
        public string CurrentPassword { get; set; }
        [NotMapped]
        public string Password { get; set; }
        [NotMapped]
        public string ConfirmPassword { get; set; }

        public Guid? Image { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDeleted { get; set; }
        public int UserType { get; set; }
        public int OTP { get; set; }

        [Display(Name = "WhatsAppNumber", ResourceType = typeof(lang))]
        public string WhatsAppNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "BirthDate", ResourceType = typeof(lang))]
        public DateTime? BirthDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? LastActivity { get; set; }
        public Guid? Signature { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<UserProperty> UserDepartment { get; set; }
        public virtual ICollection<UserPermission> UserPermissions { get; set; }

    }
}
