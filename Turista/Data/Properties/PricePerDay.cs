using System;
using System.ComponentModel.DataAnnotations;
using Turista.Resources;

namespace Turista.Data.Properties
{
    public class PricePerDay : Entity
    {
        [Display(Name = "Saturday", ResourceType = typeof(lang))]
        public double Saturday { get; set; }
        [Display(Name = "Sunday", ResourceType = typeof(lang))]
        public double Sunday { get; set; }
        [Display(Name = "Monday", ResourceType = typeof(lang))]
        public double Monday { get; set; }
        [Display(Name = "Tuesday", ResourceType = typeof(lang))]
        public double Tuesday { get; set; }
        [Display(Name = "Wednesday", ResourceType = typeof(lang))]
        public double Wednesday { get; set; }
        [Display(Name = "Thursday", ResourceType = typeof(lang))]
        public double Thursday { get; set; }
        [Display(Name = "Friday", ResourceType = typeof(lang))]
        public double Friday { get; set; }
        public Guid PropertyId { get; set; }
        public Property Property { get; set; }
    }
}