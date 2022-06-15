using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Turista.Data;
using Turista.Data.Properties;
using Turista.Models;

namespace Domain
{
    public static class Properties
    {
        public static string GetResortDefaultImage(ApplicationDbContext _context, Guid ResortId)
        {
            var resortImage = _context.ResortImages.Include(c=>c.File).Where(c => c.ResortId == ResortId).OrderByDescending(c=>c.IsPrimary);
            if (resortImage.Any())
            {
                return resortImage.FirstOrDefault().File.FileContentMin;
            }
            return Path.Combine("images", "defaultResort.jpg");
        }

        public static List<string> GetResortImages(ApplicationDbContext _context, Guid ResortId)
        {
            var resortImages = _context.ResortImages.Include(c => c.File).Where(c => c.ResortId == ResortId).OrderByDescending(c => c.IsPrimary);
            var imgs = new List<string>();
            foreach (var img in resortImages)
            {
                imgs.Add(img.File.FileContentMin);
            }

            return imgs;
        }

        public static List<string> GetPropertyImages(ApplicationDbContext _context, Guid propertyId)
        {
            var images = _context.PropertyImages.Include(c => c.File).Where(c => c.PropertyId == propertyId).OrderBy(c => c.IsPrimary);
            var imgs = new List<string>();
            foreach (var img in images)
            {
                imgs.Add(img.File.FileContentMin);
            }

            return imgs;
        }
        public static string GetPropertyDefaultImage(ApplicationDbContext _context, Guid propertyId)
        {
            var image = _context.PropertyImages.Include(c=>c.File).Where(c => c.PropertyId == propertyId).OrderByDescending(c=>c.IsPrimary);
            if (image.Any())
            {
                return image.FirstOrDefault().File.FileContentMin;
            }
            return Path.Combine("images", "defaultProperty.jpg");
        }

        public static string GetPropertyName(ApplicationDbContext _context, Property property,bool Split=true)
        {
            if (property.PropertyType==(int)enums.PropertyType.Chalets)
            {
                var resort = _context.Resorts.FirstOrDefault(c => c.Id == property.ResortId);
                if (resort != null)
                {
                    if (Split)
                        return $"<span>{property.PropertyName}</span> <br><small class='text-gray'>{resort.Name}</small>";
                    else
                        return property.PropertyName + " - " + resort.Name;
                }
            }
            return property.PropertyName;
        }

        public static string GetResortName(ApplicationDbContext _context, Property property)
        {
            if (property.PropertyType == (int)enums.PropertyType.Chalets)
            {
                var resort = _context.Resorts.FirstOrDefault(c => c.Id == property.ResortId);
                if (resort != null)
                {
                    return resort.Name;
                }
            }
            return string.Empty;
        }

        public static void getPropertyPrice(ApplicationDbContext _context, Property property, DateTime checkinDate, DateTime checkOutDate)
        {
            try
            {
                if (property.IsDayPrice)
                {
                    if (property.PricePerDays == null)
                    {
                        return;
                    }
                    var amount = 0.0;
                    var days = (int)(checkOutDate - checkinDate).TotalDays;
                    for (int i = 0; i < days; i++)
                    {
                        switch (checkinDate.DayOfWeek)
                        {
                            case DayOfWeek.Sunday:
                                amount += property.PricePerDays.Sunday;
                                break;
                            case DayOfWeek.Monday:
                                amount += property.PricePerDays.Monday;
                                break;
                            case DayOfWeek.Tuesday:
                                amount += property.PricePerDays.Tuesday;
                                break;
                            case DayOfWeek.Wednesday:
                                amount += property.PricePerDays.Wednesday;
                                break;
                            case DayOfWeek.Thursday:
                                amount += property.PricePerDays.Thursday;
                                break;
                            case DayOfWeek.Friday:
                                amount += property.PricePerDays.Friday;
                                break;
                            case DayOfWeek.Saturday:
                                amount += property.PricePerDays.Saturday;
                                break;
                        }
                        checkinDate = checkinDate.AddDays(1);
                    }

                    property.DayPrice = amount;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void getPropertyPrice(ApplicationDbContext _context, Property property, DateTime dateTime)
        {
            try
            {
                if (property.IsDayPrice)
                {
                    if (property.PricePerDays==null)
                    {
                        return;
                    }
                    var amount = 0.0;
                    switch (dateTime.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            amount += property.PricePerDays.Sunday;
                            break;
                        case DayOfWeek.Monday:
                            amount += property.PricePerDays.Monday;
                            break;
                        case DayOfWeek.Tuesday:
                            amount += property.PricePerDays.Tuesday;
                            break;
                        case DayOfWeek.Wednesday:
                            amount += property.PricePerDays.Wednesday;
                            break;
                        case DayOfWeek.Thursday:
                            amount += property.PricePerDays.Thursday;
                            break;
                        case DayOfWeek.Friday:
                            amount += property.PricePerDays.Friday;
                            break;
                        case DayOfWeek.Saturday:
                            amount += property.PricePerDays.Saturday;
                            break;
                    }
                    property.DayPrice = amount;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public static void getPropertiesPrice(ApplicationDbContext _context, List<Property> Properties, DateTime dateTime)
        {
            try
            {
                foreach (var property in Properties)
                {
                    if (!property.IsDayPrice)
                    {
                        var amount = 0.0;
                        switch (dateTime.DayOfWeek)
                        {
                            case DayOfWeek.Sunday:
                                amount += property.PricePerDays.Sunday;
                                break;
                            case DayOfWeek.Monday:
                                amount += property.PricePerDays.Monday;
                                break;
                            case DayOfWeek.Tuesday:
                                amount += property.PricePerDays.Tuesday;
                                break;
                            case DayOfWeek.Wednesday:
                                amount += property.PricePerDays.Wednesday;
                                break;
                            case DayOfWeek.Thursday:
                                amount += property.PricePerDays.Thursday;
                                break;
                            case DayOfWeek.Friday:
                                amount += property.PricePerDays.Friday;
                                break;
                            case DayOfWeek.Saturday:
                                amount += property.PricePerDays.Saturday;
                                break;
                        }
                        property.DayPrice = amount;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
