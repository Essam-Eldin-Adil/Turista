using System.ComponentModel.DataAnnotations;
using Turista.Resources;

namespace Turista.Data.General
{
    public class AppSetting:Entity
    {
        public string Version { get; set; }
        public string App { get; set; }
        [Display(Name = "AppName", ResourceType = typeof(lang))]
        public string AppName { get; set; }
        [Display(Name = "Author", ResourceType = typeof(lang))]
        public string Author { get; set; }
        [Display(Name = "AppFooter", ResourceType = typeof(lang))]
        public string AppFooter { get; set; }
        [Display(Name = "AppLoginFooter", ResourceType = typeof(lang))]
        public string AppLoginFooter { get; set; }
        [Display(Name = "AppDescription", ResourceType = typeof(lang))]
        public string AppDescription { get; set; }
        [Display(Name = "GoogleMapAPIKey", ResourceType = typeof(lang))]
        public string GoogleMapAPIKey { get; set; }


        [Display(Name = "SmtpEmail", ResourceType = typeof(lang))]
        public string SmtpEmail { get; set; }
        [Display(Name = "SmtpPassword", ResourceType = typeof(lang))]
        public string SmtpPassword { get; set; }
        [Display(Name = "SmtpHost", ResourceType = typeof(lang))]
        public string SmtpHost { get; set; }
        [Display(Name = "SmtpPort", ResourceType = typeof(lang))]
        public int SmtpPort { get; set; }
        [Display(Name = "SMSAPIUrl", ResourceType = typeof(lang))]
        public string SMSAPIUrl { get; set; }

        [Display(Name = "PaymentDescripton", ResourceType = typeof(lang))]
        public string PaymentDescripton { get; set; }
        [Display(Name = "InvoiceNotice", ResourceType = typeof(lang))]
        public string InvoiceNotice { get; set; }
        [Display(Name = "AppLink", ResourceType = typeof(lang))]
        public string AppLink { get; set; }

        //Contact Info
        [Display(Name = "ReservationPhoneNumber", ResourceType = typeof(lang))]
        public string ReservationPhoneNumber { get; set; }
        [Display(Name = "OfficeAddress", ResourceType = typeof(lang))]
        public string OfficeAddress { get; set; }
        [Display(Name = "CustomerServiceFrom", ResourceType = typeof(lang))]
        public string CustomerServiceFrom { get; set; }
        [Display(Name = "ContactPhone", ResourceType = typeof(lang))]
        public string ContactPhone { get; set; }
        [Display(Name = "CustomerServiceTo", ResourceType = typeof(lang))]
        public string CustomerServiceTo { get; set; }


        [Display(Name = "CancelReservationIn", ResourceType = typeof(lang))]
        public int CancelReservationIn { get; set; }
    }
}
