using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data.General;
using Turista.Data.Properties.PropertyDetails;
using Turista.Data.Reservation;
using Turista.Resources;

namespace Turista.Data.Properties
{
    public class Property:Entity
    {
        public Property()
        {
            PropertyImages = new List<PropertyImage>();
            Offers = new List<Offer>();
            PropertyUsers = new List<PropertyUser>();
            Reservations = new List<Turista.Data.Reservation.Reservation>();
            PropertyBookingTerms = new List<PropertyBookingTerm>();
            PropertyCancellationPolicies = new List<PropertyCancellationPolicy>();
        }
        [Display(Name = "PropertyType", ResourceType = typeof(lang))]
        public int PropertyType { get; set; }
        [Display(Name = "PropertyName", ResourceType = typeof(lang))]
        public string PropertyName { get; set; }
        [Display(Name = "ViewStatus", ResourceType = typeof(lang))]
        public bool ViewStatus { get; set; }
        [Display(Name = "Description", ResourceType = typeof(lang))]
        public string Description { get; set; }
        [Display(Name = "Location", ResourceType = typeof(lang))]
        public string Location { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        [Display(Name = "Direction", ResourceType = typeof(lang))]
        public int Direction { get; set; }
        [Display(Name = "City", ResourceType = typeof(lang))]
        public Guid CityId { get; set; }
        public City City { get; set; }
        [Display(Name = "Region", ResourceType = typeof(lang))]
        public string Region { get; set; }
        [Display(Name = "Neighborhood", ResourceType = typeof(lang))]
        public string Neighborhood { get; set; }

        [Display(Name = "UnitCode", ResourceType = typeof(lang))]
        public string PropertyCode { get; set; }
        [Display(Name = "UnitCount", ResourceType = typeof(lang))]
        public int Count { get; set; }
        [Display(Name = "PropertySpace", ResourceType = typeof(lang))]
        public int Space { get; set; }
        [Display(Name = "DepositAmount", ResourceType = typeof(lang))]
        public double DepositAmount { get; set; }
        [Display(Name = "DayPrice", ResourceType = typeof(lang))]
        public double DayPrice { get; set; }
        public bool IsDayPrice { get; set; } = true;
        public ICollection<Review> Reviews { get; set; }
        public ICollection<PropertyImage> PropertyImages { get; set; }
        public ICollection<ParameterValue> ParameterValues { get; set; }
        public ICollection<PropertyUser> PropertyUsers { get; set; }
        public ICollection<PropertyBookingTerm> PropertyBookingTerms { get; set; }
        public ICollection<PropertyCancellationPolicy> PropertyCancellationPolicies { get; set; }
        public ICollection<Turista.Data.Reservation.Reservation> Reservations { get; set; }
        public PricePerDay PricePerDays { get; set; }
        public ICollection<Offer> Offers { get; set; }

[Display(Name = "PromiseChangePrices", ResourceType = typeof(lang))]
        public bool PromiseChangePrices { get; set; }
        public bool IsConfirmed { get; set; }
        public int ReserveConetion { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }

        //Settings
        public double InsuranceAmount { get; set; }
        public DateTime EnterTime { get; set; }
        public DateTime ExitTime { get; set; }
        public bool CleanCondition { get; set; }
        public bool InsuranceCondition { get; set; }
        public bool FamilyCondition { get; set; }
        public bool MoneyTransferCondition { get; set; }
        public string OtherCondition { get; set; }
        public string ReservationManager { get; set; }
        public string ReservationPhoneNumber { get; set; }
        public int Views { get; set; }
        public Guid? ResortId { get; set; }

        public int AllowedPersons { get; set; }
        public int MaximumAllowed { get; set; }
        public bool MoreThanAllowed { get; set; }
        public double MoreThanAllowedPrice { get; set; }
        public Guid? OrginalId { get; set; }
        public int CompleteStep { get; set; }

        [NotMapped]
        public bool IsFive { get; set; }
        [NotMapped]
        public bool IsReserved { get; set; }
        [NotMapped]
        public bool IsReservedBefore { get; set; }
    }

    public class PropertyImage : Entity
    {
        public bool IsPrimary { get; set; }
        public Guid FileId { get; set; }
        public Guid PropertyId { get; set; }
        public File File { get; set; }
        public Property Property { get; set; }
    }
}
