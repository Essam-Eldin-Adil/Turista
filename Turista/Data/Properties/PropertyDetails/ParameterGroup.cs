using iQuarc.DataLocalization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.General;
using Turista.Resources;

namespace Turista.Data.Properties.PropertyDetails
{
    public class ParameterGroup : Entity
    {
        public ParameterGroup()
        {
            Parameters = new List<Parameter>();
        }
        [Required(ErrorMessageResourceName = "ValidationsRequired", ErrorMessageResourceType = typeof(lang))]
        public string Name { get; set; }
        public bool IsChild { get; set; }
        public Guid ParentId { get; set; }
        public string Icon { get; set; }
        public bool Filterable { get; set; }
        [Display(Name = "PropertyType", ResourceType = typeof(lang))]
        public int PropertyType { get; set; }
        public ICollection<Parameter> Parameters { get; set; }
        public ICollection<ParameterGroupTranslation> ParameterGroupTranslations { get; set; }
    }

    [TranslationFor(typeof(ParameterGroup))]
    public class ParameterGroupTranslation : Entity
    {
        public Guid ParameterGroupId { get; set; }
        public ParameterGroup ParameterGroup { get; set; }
        public int LanguageId { get; set; }
        public string Name { get; set; }
        public virtual Language Language { get; set; }
    }
}
