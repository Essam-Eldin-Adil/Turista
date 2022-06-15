using Turista.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Turista.Data.General
{
    public class File : Entity
    {
        [Required(ErrorMessageResourceType = typeof(lang), ErrorMessageResourceName = nameof(lang.ValidationsRequired), ErrorMessage = null)]
        public string Name { get; set; }
        [Required(ErrorMessageResourceType = typeof(lang), ErrorMessageResourceName = nameof(lang.ValidationsRequired), ErrorMessage = null)]
        public long Size { get; set; }
        [Required(ErrorMessageResourceType = typeof(lang), ErrorMessageResourceName = nameof(lang.ValidationsRequired), ErrorMessage = null)]
        public string Type { get; set; }
        public string FileContent { get; set; }
        public string FileContentMin { get; set; }
    }
}
