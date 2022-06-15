using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Turista.Data;
using Turista.Data.Properties;
using Turista.Models;

namespace Domain
{
    public static class Reservation
    {
        public static CheckedDates GetCheckedDates(HttpContext httpContext)
        {
            try
            {
                CheckedDates checkedDates = new CheckedDates();
                if (!string.IsNullOrWhiteSpace(httpContext.Session.GetString("CheckInDay")))
                {
                    checkedDates.CheckIn = getDate(int.Parse(httpContext.Session.GetString("CheckInDay")),
                        int.Parse(httpContext.Session.GetString("CheckInMonth")),
                        int.Parse(httpContext.Session.GetString("CheckInYear")));
                    checkedDates.CheckOut = getDate(int.Parse(httpContext.Session.GetString("CheckOutDay")),
                        int.Parse(httpContext.Session.GetString("CheckOutMonth")),
                        int.Parse(httpContext.Session.GetString("CheckOutYear")));
                    checkedDates.DaysCount = int.Parse(httpContext.Session.GetString("DaysCount"));
                }
                return checkedDates;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void SetCheckedDates(HttpContext httpContext,DateTime CheckIn,DateTime CheckOut)
        {
            if (CheckIn.Date==CheckOut.Date)
            {
                CheckOut = CheckOut.AddDays(1);
            }
            httpContext.Session.SetString("CheckInDay", CheckIn.Day.ToString());
            httpContext.Session.SetString("CheckInMonth", CheckIn.Month.ToString());
            httpContext.Session.SetString("CheckInYear", CheckIn.Year.ToString());
            httpContext.Session.SetString("CheckOutDay", CheckOut.Day.ToString());
            httpContext.Session.SetString("CheckOutMonth", CheckOut.Month.ToString());
            httpContext.Session.SetString("CheckOutYear", CheckOut.Year.ToString());
            var count = (int)(CheckOut - CheckIn).TotalDays;
            if (count==0)
                count++;
            httpContext.Session.SetString("DaysCount", count.ToString());

        }
        public static void Reserved(ApplicationDbContext _context, HttpContext httpContext, Property property)
        {
            var dates = GetCheckedDates(httpContext);
            property.IsReserved = _context.Reservations.Any(c => c.PropertyId == property.Id && c.Status!=(int)enums.ReservationStatus.Cancled
                 &&( (c.DateFrom.Date <= dates.CheckIn.Date && c.DateTo.Date >= dates.CheckIn.Date)
                 || (c.DateFrom.Date <= dates.CheckOut.Date && c.DateTo.Date >= dates.CheckOut.Date)));
        }

        public static void Reserved(ApplicationDbContext _context, HttpContext httpContext, List<Property> properties)
        {
            var dates = GetCheckedDates(httpContext);
            foreach (var property in properties)
            {
                property.IsReserved = _context.Reservations.Any(c => c.PropertyId == property.Id && c.Status != (int)enums.ReservationStatus.Cancled
                 && ((c.DateFrom.Date <= dates.CheckIn.Date && c.DateTo.Date >= dates.CheckIn.Date)
                 || (c.DateFrom.Date <= dates.CheckOut.Date && c.DateTo.Date >= dates.CheckOut.Date)));
            }
        }

        public static DateTime getDate(int day,int month,int year)
        {
            return new DateTime(year, month, day);
        }

        public static List<Offer> getOffers(ApplicationDbContext _context, DateTime CheckIn, DateTime CheckOut)
        {
            try
            {
                List<Offer> model = new List<Offer>();
                var date = CheckIn.Date;
                var offers = _context.Offers.Include(c=>c.Property).Where(c =>(c.DateFrom.Date <= date && c.DateTo >= date)).ToList();
                
                return model;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static List<OfferViewModel> getPropertyOffer(ApplicationDbContext _context, Property Property, DateTime CheckIn, DateTime CheckOut)
        {
            try
            {
                List<OfferViewModel> model = new List<OfferViewModel>();
                var days = (CheckOut.Date - CheckIn.Date).Days;
                var date = CheckIn.Date;
                for (int i = 0; i < days; i++)
                {
                    var offer = _context.Offers.FirstOrDefault(c =>c.PropertyId==Property.Id&& (c.DateFrom.Date <= date && c.DateTo >= date));
                    if (offer!=null)
                    {
                        if (Property.IsDayPrice)
                        {
                            var amount = Property.DayPrice - offer.Amount;
                            model.Add(new OfferViewModel
                            {
                                Date = date,
                                Amount = amount
                            });
                        }
                        else
                        {
                            var pricePerDay = _context.PricePerDays.FirstOrDefault(c => c.PropertyId == Property.Id);
                            if (pricePerDay != null)
                            {
                                switch (date.DayOfWeek)
                                {
                                    case DayOfWeek.Sunday:
                                        Property.DayPrice += pricePerDay.Sunday;
                                        break;
                                    case DayOfWeek.Monday:
                                        Property.DayPrice += pricePerDay.Monday;
                                        break;
                                    case DayOfWeek.Tuesday:
                                        Property.DayPrice += pricePerDay.Tuesday;
                                        break;
                                    case DayOfWeek.Wednesday:
                                        Property.DayPrice += pricePerDay.Wednesday;
                                        break;
                                    case DayOfWeek.Thursday:
                                        Property.DayPrice += pricePerDay.Thursday;
                                        break;
                                    case DayOfWeek.Friday:
                                        Property.DayPrice += pricePerDay.Friday;
                                        break;
                                    case DayOfWeek.Saturday:
                                        Property.DayPrice += pricePerDay.Saturday;
                                        break;
                                }
                            }
                            var amount = Property.DayPrice - offer.Amount;
                            model.Add(new OfferViewModel
                            {
                                Date = date,
                                Amount = amount
                            });
                        }
                    }
                    date = date.AddDays(1);
                }
                return model;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static List<OfferViewModel> getPropertyOfferWithoutCalc(ApplicationDbContext _context, Property Property, DateTime CheckIn, DateTime CheckOut)
        {
            try
            {
                List<OfferViewModel> model = new List<OfferViewModel>();
                var days = (CheckOut.Date - CheckIn.Date).Days;
                var date = CheckIn.Date;
                for (int i = 0; i < days; i++)
                {
                    var offer = _context.Offers.FirstOrDefault(c => c.PropertyId == Property.Id && (c.DateFrom.Date <= date && c.DateTo >= date));
                    if (offer != null)
                    {
                        if (Property.IsDayPrice)
                        {
                            //var amount = Property.DayPrice - offer.Amount;
                            model.Add(new OfferViewModel
                            {
                                Date = date,
                                Amount = offer.Amount
                            });
                        }
                        else
                        {
                            var pricePerDay = _context.PricePerDays.FirstOrDefault(c => c.PropertyId == Property.Id);
                            if (pricePerDay != null)
                            {
                                switch (date.DayOfWeek)
                                {
                                    case DayOfWeek.Sunday:
                                        Property.DayPrice += pricePerDay.Sunday;
                                        break;
                                    case DayOfWeek.Monday:
                                        Property.DayPrice += pricePerDay.Monday;
                                        break;
                                    case DayOfWeek.Tuesday:
                                        Property.DayPrice += pricePerDay.Tuesday;
                                        break;
                                    case DayOfWeek.Wednesday:
                                        Property.DayPrice += pricePerDay.Wednesday;
                                        break;
                                    case DayOfWeek.Thursday:
                                        Property.DayPrice += pricePerDay.Thursday;
                                        break;
                                    case DayOfWeek.Friday:
                                        Property.DayPrice += pricePerDay.Friday;
                                        break;
                                    case DayOfWeek.Saturday:
                                        Property.DayPrice += pricePerDay.Saturday;
                                        break;
                                }
                            }
                            var amount = Property.DayPrice - offer.Amount;
                            model.Add(new OfferViewModel
                            {
                                Date = date,
                                Amount = offer.Amount
                            });
                        }
                    }
                    date = date.AddDays(1);
                }
                return model;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static OfferViewModel getIsHasOffer(ApplicationDbContext _context, HttpContext httpContext, Property Property)
        {
            try
            {
                var dates = GetCheckedDates(httpContext);
                OfferViewModel model = new OfferViewModel();
                var date = dates.CheckIn;

                var offer = _context.Offers.FirstOrDefault(c => c.PropertyId == Property.Id && (c.DateFrom.Date <= date && c.DateTo >= date));
                if (offer!=null)
                {
                    model.OldAmount = Property.DayPrice;
                    model.Amount = offer.Amount;
                    model.Date = date;
                    model.HasOffer = true;
                }
                return model;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static double getReservationPrice(ApplicationDbContext _context, Property Property, DateTime CheckIn, DateTime CheckOut)
        {
            try
            {
                var days = (CheckOut.Date - CheckIn.Date).Days;
                if (Property.IsDayPrice)
                {
                    return Property.DayPrice * days;
                }
                var date = CheckIn.Date;
                var pricePerDay = _context.PricePerDays.FirstOrDefault(c => c.PropertyId == Property.Id);
                if (pricePerDay==null)
                {
                    return Property.DayPrice * days;
                }
                double amount = 0;
                for (int i = 0; i < days; i++)
                {
                    switch (date.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            amount += pricePerDay.Sunday;
                            break;
                        case DayOfWeek.Monday:
                            amount += pricePerDay.Monday;
                            break;
                        case DayOfWeek.Tuesday:
                            amount += pricePerDay.Tuesday;
                            break;
                        case DayOfWeek.Wednesday:
                            amount += pricePerDay.Wednesday;
                            break;
                        case DayOfWeek.Thursday:
                            amount += pricePerDay.Thursday;
                            break;
                        case DayOfWeek.Friday:
                            amount += pricePerDay.Friday;
                            break;
                        case DayOfWeek.Saturday:
                            amount += pricePerDay.Saturday;
                            break;
                    }
                    date = date.AddDays(1);
                }
                return amount;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
