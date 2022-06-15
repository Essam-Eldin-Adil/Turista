using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.Properties.PropertyDetails;
using Turista.Data.Reservation;

namespace Turista.Models
{
    public class UtilitiesViewModel
    {
        public UtilitiesViewModel()
        {
            Parameter = new Parameter();
            ParameterTranslation = new ParameterTranslation();
            ParameterGroupTranslation = new ParameterGroupTranslation();
            ParameterGroup = new ParameterGroup();
            ItemTree = new List<ItemTree>();
        }
        public ParameterTranslation ParameterTranslation { get; set; }
        public Parameter Parameter { get; set; }
        public ParameterGroupTranslation ParameterGroupTranslation { get; set; }
        public ParameterGroup ParameterGroup { get; set; }
        public List<ItemTree> ItemTree { get; set; }
    }

    public class PoliciesViewModel
    {
        public PoliciesViewModel()
        {
            BookingTerm = new BookingTerm();
            CancellationPolicy = new CancellationPolicy();
            CancellationPolicies = new List<CancellationPolicy>();
            BookingTerms = new List<BookingTerm>();
        }
        public CancellationPolicy CancellationPolicy { get; set; }
        public List<CancellationPolicy> CancellationPolicies { get; set; }
        public BookingTerm BookingTerm { get; set; }
        public List<BookingTerm> BookingTerms { get; set; }
    }

    public class ItemTree
    {
        public int Type { get; set; }
        public ParameterGroup ParameterGroup { get; set; }
        public List<ParameterGroup> ParameterGroups { get; set; }
        public bool HaveNodes { get; set; }
    }
}
