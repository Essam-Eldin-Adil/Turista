using Turista.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Turista.Data.General
{
    public class Language : Entity
    {
        [Required(ErrorMessageResourceType = typeof(lang), ErrorMessageResourceName = nameof(lang.ValidationsRequired), ErrorMessage = null)]
        public string Name { get; set; }
        [Required(ErrorMessageResourceType = typeof(lang), ErrorMessageResourceName = nameof(lang.ValidationsRequired), ErrorMessage = null)]
        public string Code { get; set; }
        [Required(ErrorMessageResourceType = typeof(lang), ErrorMessageResourceName = nameof(lang.ValidationsRequired), ErrorMessage = null)]
        public string Flag { get; set; }
    }
}
