using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Turista.Data;
using Microsoft.EntityFrameworkCore;
using Turista.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Turista.Data.Identity;
using Turista.Resources;
using Turista.Data.Reservation;
using Microsoft.AspNetCore.Authorization;
using Turista.Data.Properties;
using Turista.Data.General;

namespace Turista.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public HomeController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var cities = _context.Cities.Where(c => !c.IsDeleted).ToList();
            foreach (var city in cities)
            {
                city.ImageUrl = Url.Content("~/" + Domain.File.GetImage(_context, city.Image));
            }
            ViewBag.Cities = cities;
            return View();
        }

        public IActionResult Unit(Guid Id)
        {
            try
            {
                UnitViewModel model = new()
                {
                    Property = _context.Properties.Include(c=>c.PricePerDays).Include(c => c.Reviews).Include(c => c.ParameterValues).ThenInclude(c => c.Parameter).ThenInclude(c => c.ParameterGroup).Include(c => c.PropertyImages).ThenInclude(c => c.File).Include(c => c.City).FirstOrDefault(c => c.Id == Id)
                };
                Domain.Properties.getPropertyPrice(_context, model.Property, DateTime.Now);
                model.ParameterGroups = model.Property.ParameterValues.Select(c => c.Parameter.ParameterGroup).Distinct().ToList();
                model.Review.Cleaning = model.Property.Reviews.Sum(c => c.Cleaning) / model.Property.Reviews.Count;
                model.Review.Crew = model.Property.Reviews.Sum(c => c.Crew) / model.Property.Reviews.Count;
                model.Review.PropertyCondition = model.Property.Reviews.Sum(c => c.PropertyCondition) / model.Property.Reviews.Count;
                model.Property.Views++;
                _context.Properties.Update(model.Property);
                _context.SaveChanges();
                Domain.Reservation.SetCheckedDates(HttpContext,DateTime.Now,DateTime.Now.AddDays(1));
                Domain.Reservation.Reserved(_context, HttpContext, model.Property);
                if (User.Identity.IsAuthenticated)
                {
                    var user = _userManager.GetUserAsync(User).Result;
                    model.Property.IsReservedBefore = _context.Reservations.Any(c => c.PropertyId == Id && c.UserId == user.Id && c.Status == (int)enums.ReservationStatus.Confirmed);
                    model.Property.IsFive = _context.Fiverates.Any(c => c.PropertyId == Id && c.UserId == user.Id);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult Rate(Review model)
        {
            try
            {
                var rate = _context.Reviews.Find(model.Id);
                if (rate == null)
                {
                    var user = _userManager.GetUserAsync(User).Result;
                    model.UserId = user.Id;
                    _context.Reviews.Add(model);
                }
                else
                {
                    rate.PropertyCondition = model.PropertyCondition;
                    rate.Cleaning = model.Cleaning;
                    rate.Crew = model.Crew;
                    rate.Comment = model.Comment;
                    _context.Reviews.Update(rate);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction("Unit", new { id = model.PropertyId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Authorize()]
        public IActionResult Reserve(Guid Id)
        {
            try
            {
                UnitViewModel model = new();
                model.Property = _context.Properties.Include(c=>c.PropertyCancellationPolicies).ThenInclude(c=>c.CancellationPolicy).Include(c=>c.PropertyBookingTerms).ThenInclude(c=>c.BookingTerm).Include(c=>c.PricePerDays).Include(c => c.ParameterValues).ThenInclude(c => c.Parameter).ThenInclude(c => c.ParameterGroup).Include(c => c.PropertyImages).ThenInclude(c => c.File).Include(c => c.City).FirstOrDefault(c => c.Id == Id);
                model.ParameterGroups = model.Property.ParameterValues.Select(c => c.Parameter.ParameterGroup).Distinct().ToList();
                Domain.Reservation.Reserved(_context, HttpContext, model.Property);
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Authorize()]
        public IActionResult Payment(Guid Id)
        {
            try
            {
                UnitViewModel model = new();
                model.Property = _context.Properties.Include(c => c.ParameterValues).ThenInclude(c => c.Parameter).ThenInclude(c => c.ParameterGroup).Include(c => c.PropertyImages).ThenInclude(c => c.File).Include(c => c.City).FirstOrDefault(c => c.Id == Id);
                model.ParameterGroups = model.Property.ParameterValues.Select(c => c.Parameter.ParameterGroup).Distinct().ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Authorize()]
        public ActionResult Payment(int type, Guid propertyId, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                var user = _userManager.GetUserAsync(User).Result;
                var property = _context.Properties.Find(propertyId);
                var reservation = new Reservation
                {
                    ReservedBy = (int)enums.ReservedBy.Website,
                    DateFrom = checkIn,
                    DateTo = checkOut,
                    PropertyId = propertyId
                };
                var dates = checkOut.Date - checkIn.Date;
                reservation.TotalDays = Math.Abs(dates.Days);

                if (property.IsDayPrice)
                {
                    reservation.DayPrice = property.DayPrice;
                    reservation.TotalPrice = reservation.DayPrice * reservation.TotalDays;
                    //reservation.DayPrice = _pricePerDayRepository.Table.FirstOrDefault(c => c.UnitId == unitId);
                }
                else
                {
                    reservation.DayPrice = property.DayPrice;
                    reservation.TotalPrice = reservation.DayPrice * reservation.TotalDays;
                }

                reservation.ReservedByUser = user.FirstName + " " + user.LastName;
                reservation.UserId = user.Id;
                reservation.Status = (int)enums.ReservationStatus.New;
                reservation.ReservationNumber = 1;
                var propReservation = _context.Reservations.FirstOrDefault(c => c.PropertyId == propertyId);
                if (propReservation != null)
                {
                    reservation.ReservationNumber = propReservation.ReservationNumber + 1;
                }
                _context.Reservations.Add(reservation);
                _context.SaveChanges();
                Success(lang.ReservedSuccessfully);
                return RedirectToAction("MyReservations", "Reservations");
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IActionResult GetCheckIn(string date, Guid propertyId)
        {
            CheckedDates model = new();
            var dates = JsonConvert.DeserializeObject<Dates>(date);
            model.CheckIn = DateTime.Parse(dates.start);
            model.CheckOut = DateTime.Parse(dates.end);
            model.DaysCount = (int)(model.CheckOut - model.CheckIn).TotalDays;
            if (model.DaysCount == 0)
                model.DaysCount++;
            Domain.Reservation.SetCheckedDates(HttpContext, model.CheckIn, model.CheckOut);
            model.Property = _context.Properties.Include(c=>c.PricePerDays).FirstOrDefault(c => c.Id == propertyId);
            Domain.Properties.getPropertyPrice(_context, model.Property, model.CheckIn,model.CheckOut);
            Domain.Reservation.Reserved(_context, HttpContext, model.Property);
            return PartialView("_dates", model);
        }

        public IActionResult GetCheckInForDept(string date)
        {
            CheckedDates model = new();
            var dates = JsonConvert.DeserializeObject<Dates>(date);
            model.CheckIn = DateTime.Parse(dates.start);
            model.CheckOut = DateTime.Parse(dates.end);
            Domain.Reservation.SetCheckedDates(HttpContext, model.CheckIn, model.CheckOut);
            return PartialView("_datesForDept", model);
        }

        public IActionResult GetReviews(Guid propertyId)
        {
            var model = new ReviewViewModel
            {
                totalReviewCount = _context.Reviews.Count(c => c.PropertyId == propertyId && !c.IsReplay),
                Property = new Property(),
                Reviews = _context.Reviews.Include(c => c.User).Where(c => c.PropertyId == propertyId && !c.IsReplay).Take(2).ToList()
            };
            if (User.Identity.IsAuthenticated)
            {
                var user = _userManager.GetUserAsync(User).Result;
                model.Property.IsReservedBefore = _context.Reservations.Any(c => c.PropertyId == propertyId && c.UserId == user.Id && c.Status == (int)enums.ReservationStatus.Confirmed);
            }
            foreach (var review in model.Reviews)
            {
                review.Replyeds = _context.Reviews.Include(c => c.User).Where(c => c.ReplayedToId == review.Id).ToList();
            }
            return PartialView("_reviews", model);
        }

        public IActionResult GetMoreReviews(Guid propertyId, int index)
        {
            var model = _context.Reviews.Include(c => c.User).Where(c => c.PropertyId == propertyId && !c.IsReplay).Skip(index).Take(2).ToList();
            foreach (var review in model)
            {
                review.Replyeds = _context.Reviews.Include(c => c.User).Where(c => c.ReplayedToId == review.Id).ToList();
            }
            return PartialView("_moreReviews", model);
        }

        public IActionResult Search(List<Guid> Cities, List<int> ProprtyType, DateTime dateCheckIn, DateTime dateCheckOut)
        {
            try
            {
                SearchViewModel model = new();
                model.Cities = _context.Cities.Where(c => !c.IsDeleted).ToList();
                model.ParameterGroups = _context.ParameterGroups.Include(c => c.Parameters).Where(c => c.Filterable && !c.IsDeleted).ToList();
                if (dateCheckIn.Date<DateTime.Now.Date)
                {
                    dateCheckIn = DateTime.Now.Date;
                    dateCheckOut = DateTime.Now.AddDays(1).Date;
                }
                model.DateFrom = dateCheckIn;
                model.DateTo = dateCheckOut;
                DataValidation(model, Cities, ProprtyType);
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private void DataValidation(SearchViewModel model, List<Guid> cities, List<int> ProprtyType)
        {
            if (cities.Count == 0)
                cities.Add(Guid.Empty);
            model.City = cities;
            if (ProprtyType.Count == 0)
                ProprtyType.Add((int)enums.PropertyType.All);
            model.ProprtyType = ProprtyType;
            //if (string.IsNullOrEmpty(Date))
            //{
            //    model.DateFrom = DateTime.Now;
            //    model.DateTo = DateTime.Now.AddDays(1);
            //}
            //else
            //{
            //    try
            //    {
            //        var dates = Date.Split(" - ");
            //        model.DateFrom = DateTime.Parse(dates[0]);
            //        model.DateTo = DateTime.Parse(dates[1]);
            //    }
            //    catch
            //    {
            //        model.DateFrom = DateTime.Now;
            //        model.DateTo = DateTime.Now.AddDays(1);
            //    }
            //}
        }

        public IActionResult SearchProperty(DateTime? dateCheckIn, DateTime? dateCheckOut, string CityId, string propertyName, string neighborhoodName, string unitCode, int? directions, int? sorting, string propType, string price, string parameters, string inputIds, string inputValues, int pageNumber = 1, bool offers = false)
        {
            try
            {
                List<Guid> cities = new List<Guid>();
                List<int> propTypes = new List<int>();
                if (!string.IsNullOrEmpty(CityId))
                {
                    var cityIds = CityId.Split(",");
                    foreach (var item in cityIds)
                    {
                        cities.Add(Guid.Parse(item));
                    }
                }

                if (!string.IsNullOrEmpty(propType))
                {
                    var prop = propType.Split(",");
                    foreach (var item in prop)
                    {
                        propTypes.Add(int.Parse(item));
                    }
                }
                double PriceFrom = 0;
                double PriceTo = 10000000;
                if (!string.IsNullOrEmpty(price))
                {
                    PriceFrom = double.Parse(price.Split(";")[0]);
                    PriceTo = double.Parse(price.Split(";")[1]);
                }
                var parms = new List<Guid>();
                var inputIdsArr = new List<Guid>();
                var inputValuesArr = new List<string>();
                if (!string.IsNullOrEmpty(parameters))
                {
                    parameters = parameters.Substring(0, parameters.Length - 1);
                    foreach (var item in parameters.Split(",").ToList())
                    {
                        parms.Add(Guid.Parse(item));
                    }
                }
                if (!string.IsNullOrEmpty(inputIds))
                {
                    inputIds = inputIds.Substring(0, inputIds.Length - 1);
                    foreach (var item in inputIds.Split(",").ToList())
                    {
                        inputIdsArr.Add(Guid.Parse(item));
                    }
                }
                if (!string.IsNullOrEmpty(inputValues))
                {
                    inputValues = inputValues.Substring(0, inputValues.Length - 1);
                    var list = inputValues.Split(",").ToList();
                    foreach (var item in list)
                    {
                        var index = 0;
                        if (!string.IsNullOrEmpty(item))
                        {
                            inputValuesArr.Add(item);
                        }
                        else
                        {
                            var id = inputIdsArr[index];
                            inputIdsArr.Remove(id);
                        }
                        index++;
                    }
                }
                PropertyViewModel model = new PropertyViewModel();
                model.PageSize = 10;
                var excludeRecord = (model.PageSize * pageNumber) - model.PageSize;
                model.PageNumber = pageNumber;

                if (offers)
                {
                    model.Properties = _context.Offers.Include(c => c.Property.City).Where(c => c.Property.ViewStatus && c.Property.IsConfirmed &&
                                         (string.IsNullOrEmpty(propertyName) || c.Property.PropertyName.Contains(propertyName)) &&
                                         (string.IsNullOrEmpty(neighborhoodName) || c.Property.Neighborhood.Contains(neighborhoodName)) &&
                                         (string.IsNullOrEmpty(unitCode) || c.Property.PropertyCode.Contains(unitCode)) &&
                                          ((propTypes.Count == 0 || propTypes.Contains(0)) || propTypes.Contains(c.Property.PropertyType)) &&
                                         ((cities.Count == 0 || cities.Contains(Guid.Empty)) || cities.Contains(c.Property.CityId))
                                         && (c.Property.DayPrice >= PriceFrom && c.Property.DayPrice <= PriceTo) &&
                                         (directions == 0 || c.Property.Direction == directions) &&
                                         (parms.Count == 0 || c.Property.ParameterValues.Any(m => parms.Contains(m.ParameterId)))).Skip(excludeRecord).Take(model.PageSize).Select(c=>c.Property).ToList();

                    model.TotalRecord = _context.Offers.Count(c => c.Property.ViewStatus && c.Property.IsConfirmed &&
                                         (string.IsNullOrEmpty(propertyName) || c.Property.PropertyName.Contains(propertyName)) &&
                                         (string.IsNullOrEmpty(neighborhoodName) || c.Property.Neighborhood.Contains(neighborhoodName)) &&
                                         (string.IsNullOrEmpty(unitCode) || c.Property.PropertyCode.Contains(unitCode)) &&
                                          ((propTypes.Count == 0 || propTypes.Contains(0)) || propTypes.Contains(c.Property.PropertyType)) &&
                                         ((cities.Count == 0 || cities.Contains(Guid.Empty)) || cities.Contains(c.Property.CityId))
                                         && (c.Property.DayPrice >= PriceFrom && c.Property.DayPrice <= PriceTo) &&
                                         (directions == 0 || c.Property.Direction == directions) &&
                                         (parms.Count == 0 || c.Property.ParameterValues.Any(m => parms.Contains(m.ParameterId))));
                }
                else
                {
                    model.Properties = _context.Properties.Include(c => c.City).Where(c => c.ViewStatus && c.IsConfirmed &&
                      (string.IsNullOrEmpty(propertyName) || c.PropertyName.Contains(propertyName)) &&
                      (string.IsNullOrEmpty(neighborhoodName) || c.Neighborhood.Contains(neighborhoodName)) &&
                      (string.IsNullOrEmpty(unitCode) || c.PropertyCode.Contains(unitCode)) &&
                       ((propTypes.Count == 0 || propTypes.Contains(0)) || propTypes.Contains(c.PropertyType)) &&
                      ((cities.Count == 0 || cities.Contains(Guid.Empty)) || cities.Contains(c.CityId))
                      && (c.DayPrice >= PriceFrom && c.DayPrice <= PriceTo) &&
                      (directions == 0 || c.Direction == directions) &&
                      (parms.Count == 0 || c.ParameterValues.Any(m => parms.Contains(m.ParameterId)))).Skip(excludeRecord).Take(model.PageSize).ToList();

                   

                    model.TotalRecord = _context.Properties.Count(c => c.ViewStatus && c.IsConfirmed &&
                      (string.IsNullOrEmpty(propertyName) || c.PropertyName.Contains(propertyName)) &&
                      (string.IsNullOrEmpty(neighborhoodName) || c.Neighborhood.Contains(neighborhoodName)) &&
                      (string.IsNullOrEmpty(unitCode) || c.PropertyCode.Contains(unitCode)) &&
                       (propTypes.Count == 0 || propTypes.Contains(c.PropertyType)) &&
                      (c.DayPrice >= PriceFrom && c.DayPrice <= PriceTo) &&
                      (directions == 0 || c.Direction == directions) &&
                      (parms.Count == 0 || c.ParameterValues.Any(m => parms.Contains(m.ParameterId))));
                }
                switch (sorting)
                {
                    case (int)enums.SortingList.ViewedListing:
                        model.Properties = model.Properties.OrderBy(c => c.Views).ToList();
                        break;
                    case (int)enums.SortingList.AZ_Listed:
                        model.Properties = model.Properties.OrderBy(c => c.PropertyName).ToList();
                        break;
                    case (int)enums.SortingList.PriceHighestFirst:
                        model.Properties = model.Properties.OrderByDescending(c => c.DayPrice).ToList();
                        break;
                    case (int)enums.SortingList.PriceLowestFirst:
                        model.Properties = model.Properties.OrderBy(c => c.DayPrice).ToList();
                        break;
                    default:
                        break;
                }
                Domain.Reservation.SetCheckedDates(HttpContext, (DateTime)dateCheckIn, (DateTime)dateCheckOut);
                Domain.Reservation.Reserved(_context, HttpContext, model.Properties);
                return PartialView("_propertiesFiltered", model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IActionResult Fiverates()
        {
            try
            {
                try
                {
                    var user = _userManager.GetUserAsync(User).Result;
                    var model = _context.Fiverates.Include(c => c.Property.City).Include(c => c.Property.PropertyImages).ThenInclude(c => c.File).Where(c => c.UserId == user.Id).ToList();
                    return View(model);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult fiverate(Guid propertyId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(null);
            }
            var user = _userManager.GetUserAsync(User).Result;
            var five = _context.Fiverates.FirstOrDefault(c => c.PropertyId == propertyId && c.UserId == user.Id);
            if (five == null)
            {
                five = new Data.Properties.Fiverate
                {
                    PropertyId = propertyId,
                    UserId = user.Id
                };
                _context.Fiverates.Add(five);
                _context.SaveChanges();
                return Json(true);
            }
            else
            {
                _context.Fiverates.Remove(five);
                _context.SaveChanges();
                return Json(false);
            }
        }

        public IActionResult RemoveFivrate(Guid id)
        {
            var user = _userManager.GetUserAsync(User).Result;
            var five = _context.Fiverates.FirstOrDefault(c => c.PropertyId == id && c.UserId == user.Id);
            if (five != null)
            {
                _context.Fiverates.Remove(five);
                _context.SaveChanges();
            }
            return RedirectToAction("Fiverates");
        }

        [Authorize()]
        public IActionResult Profile()
        {
            try
            {
                var user = _userManager.GetUserAsync(User).Result;
                return View(user);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost]
        [Authorize()]
        public IActionResult SaveProfile(User model)
        {
            try
            {
                var user = _context.Users.Find(model.Id);
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                if (Request.Form.Files.Count > 0)
                {
                    var filesId = Domain.File.Upload("UserImages", _context, Request.Form.Files);
                    user.Image = filesId[0];
                }
                _context.Users.Update(user);
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
            }
            catch (Exception ex)
            {

                throw;
            }
            return RedirectToAction("Profile");
        }

        public class Dates
        {
            public string start { get; set; }
            public string end { get; set; }
        }

        [HttpGet]
        public IActionResult ChangeLanguage(string code, string flag, string name, string direction)
        {
            base.Language(code, flag, name, direction);
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpGet]
        public IActionResult Resorts()
        {
            try
            {
                ViewBag.TotalCount = _context.Resorts.Count(c => c.IsConfirmed && c.ViewStatus);
                return View();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        public IActionResult getResorts(int pageNumber)
        {
            var pageSize = 10;
            var model = _context.Resorts.Include(c => c.ResortImages).ThenInclude(c => c.File).Include(c => c.City).Where(c => c.IsConfirmed && c.ViewStatus).Skip(pageNumber).Take(pageSize).ToList();
            return PartialView("_resorts", model);
        }


        [HttpGet]
        public IActionResult Chalets()
        {
            try
            {
                ViewBag.TotalCount = _context.Properties.Count(c => c.PropertyType == (int)enums.PropertyType.Chalets && c.ViewStatus);
                return View();
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpGet]
        public IActionResult getProperty(int type, int pageNumber, string dates)
        {
            if (!string.IsNullOrEmpty(dates))
            {
                var date = JsonConvert.DeserializeObject<Dates>(dates);
                Domain.Reservation.SetCheckedDates(HttpContext, DateTime.Parse(date.start), DateTime.Parse(date.end));
            }
            var pageSize = 10;
            PropertyForDept model = new PropertyForDept();
            model.Properties = _context.Properties.Include(c => c.City).Where(c => c.PropertyType == type && !c.IsDeleted && c.IsConfirmed && c.ViewStatus).Skip(pageNumber).Take(pageSize).ToList();
            model.PageNumber = pageNumber + pageSize;
            model.TotalCount = _context.Properties.Count(c => c.PropertyType == type);
            model.hasMore = model.TotalCount > model.PageNumber;
            Domain.Reservation.Reserved(_context, HttpContext, model.Properties);
            return PartialView("_propDept", model);
        }

        [HttpGet]
        public IActionResult Resort(Guid id)
        {
            try
            {
                ViewBag.TotalCount = _context.Properties.Count(c => c.IsConfirmed && c.PropertyType == (int)enums.PropertyType.Chalets && c.ResortId == id && c.ViewStatus);
                SingleResortViewModel model = new();
                model.Resort = _context.Resorts.Include(c => c.ResortParameterValues).ThenInclude(c => c.Parameter).ThenInclude(c => c.ParameterGroup).Include(c => c.ResortImages).ThenInclude(c => c.File).Include(c => c.City).FirstOrDefault(c => c.Id == id);
                model.ParameterGroups = model.Resort.ResortParameterValues.Select(c => c.Parameter.ParameterGroup).Distinct().ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        public IActionResult getResortProperty(int type, int pageNumber, string dates, Guid resortId)
        {
            if (!string.IsNullOrEmpty(dates))
            {
                var date = JsonConvert.DeserializeObject<Dates>(dates);
                Domain.Reservation.SetCheckedDates(HttpContext, DateTime.Parse(date.start), DateTime.Parse(date.end));
            }
            var pageSize = 10;
            PropertyForDept model = new PropertyForDept();
            model.Properties = _context.Properties.Include(c => c.City).Where(c => c.ResortId == resortId && c.PropertyType == type && !c.IsDeleted && c.IsConfirmed && c.ViewStatus).Skip(pageNumber).Take(pageSize).ToList();
            model.PageNumber = pageNumber + pageSize;
            model.TotalCount = _context.Properties.Count(c => c.PropertyType == type && c.ResortId == resortId && c.IsConfirmed);
            model.hasMore = model.TotalCount > model.PageNumber;
            Domain.Reservation.Reserved(_context, HttpContext, model.Properties);
            return PartialView("_propDept", model);
        }


        [HttpGet]
        public IActionResult RestHouses()
        {
            try
            {
                ViewBag.TotalCount = _context.Properties.Count(c => c.PropertyType == (int)enums.PropertyType.Rest && c.ViewStatus);
                return View();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet]
        public IActionResult Villas()
        {
            try
            {
                ViewBag.TotalCount = _context.Properties.Count(c => c.PropertyType == (int)enums.PropertyType.Villa && c.ViewStatus);
                return View();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet]
        public IActionResult Offers()
        {
            try
            {
                //var checkedDates = Domain.Reservation.GetCheckedDates(HttpContext);
                //ViewBag.TotalCount = _context.Offers.Include(c => c.Property).ThenInclude(c => c.City).Count(c => (c.DateFrom.Date <= checkedDates.CheckIn && c.DateTo >= checkedDates.CheckIn) && !c.IsDeleted && c.Property.IsConfirmed && c.Property.ViewStatus);
                //return View();
                SearchViewModel model = new();
                model.Cities = _context.Cities.Where(c => !c.IsDeleted).ToList();
                model.ParameterGroups = _context.ParameterGroups.Include(c => c.Parameters).Where(c => c.Filterable && !c.IsDeleted).ToList();
                model.DateFrom = DateTime.Now;
                model.DateTo = DateTime.Now.AddDays(1);
                DataValidation(model, new List<Guid>(), new List<int>());
                return View(model);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet]
        public IActionResult getPropertyOffers(int type, int pageNumber, string dates)
        {
            if (!string.IsNullOrEmpty(dates))
            {
                var date = JsonConvert.DeserializeObject<Dates>(dates);
                Domain.Reservation.SetCheckedDates(HttpContext, DateTime.Parse(date.start), DateTime.Parse(date.end));
            }
            var checkedDates = Domain.Reservation.GetCheckedDates(HttpContext);
            var pageSize = 10;
            PropertyForDept model = new PropertyForDept();
            model.Properties = _context.Offers.Include(c => c.Property).ThenInclude(c => c.City).Where(c => (c.DateFrom.Date <= checkedDates.CheckIn && c.DateTo >= checkedDates.CheckIn) && !c.IsDeleted && c.Property.IsConfirmed && c.Property.ViewStatus).Skip(pageNumber).Take(pageSize).Select(c => c.Property).ToList();
            model.PageNumber = pageNumber + pageSize;
            model.TotalCount = _context.Properties.Count(c => c.PropertyType == type);
            model.hasMore = model.TotalCount > model.PageNumber;
            Domain.Reservation.Reserved(_context, HttpContext, model.Properties);
            return PartialView("_propDept", model);
        }


        [HttpGet]
        public IActionResult ContactUs()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult ContactUs(ContactUs model)
        {
            try
            {
                _context.ContactsUs.Add(model);
                _context.SaveChanges();
                Success(lang.MessageHasBeenSent);
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
            model = new ContactUs();
            return RedirectToAction(nameof(ContactUs));
        }
        public IActionResult Redircet()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction(nameof(Index));
                }
                var user = _userManager.GetUserAsync(User).Result;
                return user.UserType switch
                {
                    (int)enums.UserType.Admin => RedirectToAction("Index", "CPanel"),
                    (int)enums.UserType.BookAdmin => RedirectToAction("Properties", "UserAccount"),
                    (int)enums.UserType.BookUser => RedirectToAction("Properties", "UserAccount"),
                    (int)enums.UserType.EndUser => RedirectToAction(nameof(Index)),
                    _ => RedirectToAction(nameof(Index)),
                };
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
