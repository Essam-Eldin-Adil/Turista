using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Turista.Models
{
    public class enums
    {
        public enum PropertySteps{
            Info,
            Services,
            Images,
            ContactInfo
            }
        public enum ReservedBy
        {
            ProprityUser,
            SystemAdmin,
            Website,
            Application
        }
        public enum PropertyType
        {
            All,
            Chalets,
            Rest,
            Villa,
            Resort
        }
        public enum LogType
        {
            Database = 1,
            System,
            User
        }

        public enum Direction
        {
            North = 1,
            South,
            East,
            West,
            Northeast,
            Southeast,
            NorthWest,
            Southwest
        }

        public enum ReservationStatus
        {
            New,
            Confirmed,
            Cancled
        }

        public enum ReserveConetion
        {
            FamilyOnly,
            FamilyAndSingles
        }

        public enum ParameterType
        {
            Text,
            Checkbox,
            Number
        }

        public enum PaymentMethod
        {
            Cash,
            CreditCard,
            PaymentInvoice
        }

        public enum SortingList
        {
            AllListing=1,
            ReviewListing,
            LocationListing,
            ViewedListing,
            AZ_Listed,
            PriceLowestFirst,
            PriceHighestFirst
        }

        public enum CanceledReson
        {
            CanceledByUser
        }

        public enum UserType
        {
            Admin = 1,
            BookAdmin,
            BookUser,
            EndUser
        }
    }
}
