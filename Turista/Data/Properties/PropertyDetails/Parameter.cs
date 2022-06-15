using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Turista.Resources;
using System.Threading.Tasks;
using iQuarc.DataLocalization;
using Turista.Data.General;

namespace Turista.Data.Properties.PropertyDetails
{
    public class Parameter : Entity
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public int Index { get; set; }
        public string Icon { get; set; }
        public ParameterGroup ParameterGroup { get; set; }
        public Guid ParameterGroupId { get; set; }
        public ICollection<ParameterTranslation> ParameterTranslations { get; set; }
    }

    [TranslationFor(typeof(Parameter))]
    public class ParameterTranslation : Entity
    {
        public Guid ParameterId { get; set; }
        public Parameter Parameter { get; set; }
        public int LanguageId { get; set; }
        public string Name { get; set; }
        public virtual Language Language { get; set; }
    }
}
