using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data;
using Turista.Data.Identity;
using Turista.Data.Properties;
using Turista.Data.Properties.PropertyDetails;
using Turista.Data.Reservation;
using Turista.Data.Resorts;
using Turista.Data.Resorts.PropertyDetails;
using Turista.Models;
using Turista.Resources;

namespace Turista.Controllers
{
    [Authorize(Roles = "PropertyAdmin")]
    public class UserAccountController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public UserAccountController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            try
            {
                MyPropretiesViewModels model = new();
                var user = _userManager.GetUserAsync(User).Result;
                model.Properties = _context.PropertyUsers.Include(c => c.Property).Where(c => c.UserId == user.Id).Select(c => c.Property).ToList();
                model.Resorts = _context.ResortUsers.Include(c => c.Resort).Where(c => c.UserId == user.Id).Select(c => c.Resort).ToList();
                return View(model);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IActionResult Properties()
        {
            try
            {
                MyPropretiesViewModels model = new();
                var user = _userManager.GetUserAsync(User).Result;
                model.Properties = _context.PropertyUsers.Include(c => c.Property).Where(c => c.UserId == user.Id && c.Property.PropertyType != (int)enums.PropertyType.Chalets).Select(c => c.Property).ToList();
                model.Resorts = _context.ResortUsers.Include(c => c.Resort).ThenInclude(c => c.ResortImages).ThenInclude(c => c.File).Where(c => c.UserId == user.Id).Select(c => c.Resort).ToList();
                return View(model);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IActionResult Resort(Guid? id)
        {
            try
            {
                var cities = _context.Cities.Where(c => !c.IsDeleted).ToList();
                ViewBag.Cities = new SelectList(cities, "Id", "CityName");

                var resort = new Resort();
                if (id == null)
                {
                    return View(resort);
                }
                var user = _userManager.GetUserAsync(User).Result;
                resort = _context.Resorts.Include(c => c.ResortImages).ThenInclude(c => c.File).FirstOrDefault(c => c.Id == id && c.ResortUsers.Select(c => c.UserId).Contains(user.Id));
                if (resort == null)
                {
                    resort = new Resort();
                }

                return View(resort);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult Resort(Resort model)
        {
            var trans = _context.Database.BeginTransaction();
            try
            {
                var cities = _context.Cities.Where(c => !c.IsDeleted).ToList();
                ViewBag.Cities = new SelectList(cities, "Id", "CityName");

                if (!_context.Resorts.Any(c => c.Id == model.Id))
                {
                    var user = _userManager.GetUserAsync(User).Result;
                    model.CompleteStep = (int)enums.PropertySteps.Info;
                    _context.Resorts.Add(model);
                    _context.SaveChanges();
                    var resortuser = new ResortUser();
                    resortuser.ResortId = model.Id;
                    resortuser.IsAdmin = true;
                    resortuser.SendWhatsAppNotifications = true;
                    resortuser.UserId = user.Id;
                    _context.ResortUsers.Add(resortuser);
                }
                else
                {
                    _context.Update(model);
                }
                _context.SaveChanges();
                trans.Commit();
                var ActionName = "";
                if (model.CompleteStep == (int)enums.PropertySteps.ContactInfo)
                {
                    ActionName = "Property";
                }
                else
                {
                    return RedirectToAction("ResortUtilities", new { id = model.Id });
                }
                return View(model);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult AddResortImage(Guid resortId)
        {
            try
            {
                var isHasImages = _context.ResortImages.Any(c => c.ResortId == resortId);
                var files = Request.Form.Files;
                var filesId = Domain.File.Upload("ResortsImages", _context, files);
                foreach (var id in filesId)
                {
                    var resortImage = new ResortImage();
                    resortImage.ResortId = resortId;
                    resortImage.FileId = id;
                    if (!isHasImages)
                    {
                        isHasImages = true;
                        if (filesId.IndexOf(id) == 0)
                        {
                            resortImage.IsPrimary = true;
                        }
                    }
                    _context.ResortImages.Add(resortImage);
                }
                _context.SaveChanges();
                var ActionName = "";
                var resort = _context.Resorts.Find(resortId);
                if (resort.CompleteStep == (int)enums.PropertySteps.ContactInfo)
                {
                    ActionName = "ResortImages";
                }
                else
                {
                    ActionName = "ResortProperties";
                }
                if (resort.CompleteStep < (int)enums.PropertySteps.Images)
                {
                    resort.CompleteStep = (int)enums.PropertySteps.Images;
                    _context.Update(resort);
                    _context.SaveChanges();
                }
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(ActionName, new { id = resortId });
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult RemoveResortImage(Guid id, Guid resortId)
        {
            Domain.File.Remove(_context, id);
            var resortImage = _context.ResortImages.FirstOrDefault(c => c.FileId == id);
            if (resortImage != null)
            {
                _context.ResortImages.Remove(resortImage);
            }
            _context.SaveChanges();
            Success(lang.AlertDataSavedSuccessfully);
            return RedirectToAction("ResortImages", new { id = resortId });
        }

        [HttpPost]
        public IActionResult SetDefaultResortImage(Guid id, Guid resortId)
        {
            var trans = _context.Database.BeginTransaction();
            //Domain.File.Remove(_context, id);
            var resortImage = _context.ResortImages.FirstOrDefault(c => c.FileId == id);
            if (resortImage != null)
            {
                var defaultImage = _context.ResortImages.FirstOrDefault(c => c.ResortId == resortId && c.IsPrimary);
                if (defaultImage != null)
                {
                    defaultImage.IsPrimary = false;
                    _context.Update(defaultImage);
                }

                resortImage.IsPrimary = true;
                _context.Update(resortImage);
            }
            _context.SaveChanges();
            trans.Commit();
            Success(lang.AlertDataSavedSuccessfully);
            return RedirectToAction("ResortImages", new { id = resortId });
        }

        public IActionResult GetResortUtilities(Guid resortId)
        {
            var model = new ResortViewModel();
            model.ParameterGroups = _context.ParameterGroups.Where(c => c.PropertyType == (int)enums.PropertyType.Resort || c.PropertyType == (int)enums.PropertyType.All).Include(c => c.Parameters).ToList();
            model.ResortParameterValues = _context.ResortParameterValues.Where(c => c.ResortId == resortId).ToList();
            return PartialView("_resortUtilities", model);
        }

        [HttpPost]
        public IActionResult SaveResortUtilities(Guid ResortId, string[] inputValues, Guid[] inputIds, Guid[] checkboxes)
        {
            try
            {
                BindParameter(inputValues, inputIds, checkboxes, ResortId);
                var resort = _context.Resorts.Find(ResortId);
                var ActionName = "";
                if (resort.CompleteStep == (int)enums.PropertySteps.ContactInfo)
                {
                    ActionName = "ResortUtilities";
                }
                else
                {
                    ActionName = "ResortImages";
                }
                if (resort.CompleteStep < (int)enums.PropertySteps.Services)
                {
                    resort.CompleteStep = (int)enums.PropertySteps.Services;
                    _context.Update(resort);
                    _context.SaveChanges();
                }
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(ActionName, new { id = ResortId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void BindParameter(IReadOnlyList<string> inputValues, IReadOnlyList<Guid> inputIds, IEnumerable<Guid> checkboxes, Guid ResortId)
        {
            try
            {
                var trans = _context.Database.BeginTransaction();
                var parametersValues = _context.ResortParameterValues.Where(c => c.ResortId == ResortId).ToList();
                _context.ResortParameterValues.RemoveRange(parametersValues);

                for (var i = 0; i < inputIds.Count; i++)
                {
                    var paramValues = new ResortParameterValue
                    {
                        ResortId = ResortId,
                        ParameterId = inputIds[i],
                        Value = inputValues[i]
                    };
                    _context.ResortParameterValues.Add(paramValues);
                }

                foreach (var checkbox in checkboxes)
                {
                    var paramValues = new ResortParameterValue
                    {
                        ResortId = ResortId,
                        ParameterId = checkbox,
                        Value = "1"
                    };
                    _context.ResortParameterValues.Add(paramValues);
                }
                _context.SaveChanges();
                trans.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public IActionResult GetResortChalets(Guid resortId)
        {
            try
            {
                var model = _context.Properties.Where(c => c.ResortId == resortId).ToList();
                return PartialView("_chaletList", model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        /// <summary>
        /// Property Section
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public IActionResult Property(Guid? id, int? type, Guid? ResortId)
        {
            try
            {
                var cities = _context.Cities.Where(c => !c.IsDeleted).ToList();
                ViewBag.Cities = new SelectList(cities, "Id", "CityName");

                var property = new Property();
                if (id == null)
                {
                    if (type != null)
                    {
                        property.PropertyType = (int)type;
                        property.ResortId = ResortId;

                    }
                    return View(property);
                }
                var user = _userManager.GetUserAsync(User).Result;
                property = _context.Properties.Include(c => c.PropertyBookingTerms).Include(v => v.PropertyCancellationPolicies).Include(c => c.PricePerDays).FirstOrDefault(c => c.Id == id && c.PropertyUsers.Select(c => c.UserId).Contains(user.Id));
                ViewBag.BookingTerms = _context.BookingTerms.Where(c => (type == null || c.PropertyType == type) || c.PropertyType == (int)enums.PropertyType.All).ToList();
                ViewBag.CancellationPolicies = _context.CancellationPolicies.Where(c => (type == null || c.PropertyType == type) || c.PropertyType == (int)enums.PropertyType.All).ToList();
                if (property == null)
                {
                    property = new Property();
                    if (type != null)
                    {
                        property.PropertyType = (int)type;
                    }
                }
                ViewBag.Similers = _context.Properties.Where(c => c.OrginalId == id).ToList();
                return View(property);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult Property(Property model, Guid[] Policies, Guid[] Terms, bool ChangeSimiler)
        {
            var trans = _context.Database.BeginTransaction();
            try
            {
                ViewBag.BookingTerms = _context.BookingTerms.Where(c => (c.PropertyType == model.PropertyType) || c.PropertyType == (int)enums.PropertyType.All).ToList();
                ViewBag.CancellationPolicies = _context.CancellationPolicies.Where(c => (c.PropertyType == model.PropertyType) || c.PropertyType == (int)enums.PropertyType.All).ToList();

                var cities = _context.Cities.Where(c => !c.IsDeleted).ToList();
                ViewBag.Cities = new SelectList(cities, "Id", "CityName");
                //var ActionName = "";
                if (!_context.Properties.Any(c => c.Id == model.Id))
                {
                    model.CompleteStep=(int)enums.PropertySteps.Info;
                    var user = _userManager.GetUserAsync(User).Result;
                    _context.Properties.Add(model);
                    _context.SaveChanges();
                    var resortuser = new PropertyUser();
                    resortuser.PropertyId = model.Id;
                    resortuser.IsAdmin = true;
                    resortuser.SendWhatsAppNotifications = true;
                    resortuser.UserId = user.Id;
                    _context.PropertyUsers.Add(resortuser);
                }
                else
                {
                    //model.IsConfirmed = _context.Properties.Find(model.Id).IsConfirmed;
                    _context.Update(model);
                }
                if (model.IsDayPrice)
                {
                    var isExist = _context.PricePerDays.Any(c => c.PropertyId == model.Id);
                    if (!isExist)
                    {
                        //model.PricePerDays = new PricePerDay();
                        _context.PricePerDays.Add(model.PricePerDays);
                    }
                    else
                    {
                        _context.Update(model.PricePerDays);
                    }
                }
                // model = _context.Properties.Include(c => c.PropertyBookingTerms).Include(v => v.PropertyCancellationPolicies).FirstOrDefault(c => c.Id == model.Id);

                _context.PropertyCancellationPolicies.RemoveRange(_context.PropertyCancellationPolicies.Where(c => c.PropertyId == model.Id));
                _context.PropertyBookingTerms.RemoveRange(_context.PropertyBookingTerms.Where(c => c.PropertyId == model.Id));
                foreach (var policyId in Policies)
                {
                    var policy = new PropertyCancellationPolicy
                    {
                        PropertyId = model.Id,
                        CancellationPolicyId = policyId
                    };
                    _context.Add(policy);
                }
                foreach (var termId in Terms)
                {
                    var term = new PropertyBookingTerm
                    {
                        PropertyId = model.Id,
                        BookingTermId = termId
                    };
                    _context.Add(term);
                }
                if (ChangeSimiler)
                {
                    var similers = _context.Properties.Where(c => c.OrginalId == model.Id && !c.IsDeleted).ToList();
                    foreach (var simProperty in similers)
                    {
                        var sim = model;
                        sim.Id = simProperty.Id;
                        _context.Update(sim);
                    }
                }
                _context.SaveChanges();
                trans.Commit();
                if (model.CompleteStep == (int)enums.PropertySteps.ContactInfo)
                {
                    //ActionName = "Property";
                }
                else
                {
                    return RedirectToAction("PropertyUtilities", new { id = model.Id });
                }
                Success(lang.AlertDataSavedSuccessfully);
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        public IActionResult SimillerProperty(Guid propertyId, int count)
        {
            var trans = _context.Database.BeginTransaction();
            try
            {
                var property = _context.Properties.Include(c=>c.ParameterValues).Include(c=>c.PricePerDays).Include(c => c.PropertyBookingTerms).Include(c => c.PropertyCancellationPolicies).Include(c => c.PropertyImages).ThenInclude(c => c.File).Include(c => c.PropertyUsers).FirstOrDefault(c => c.Id == propertyId);
                for (int i = 0; i < count; i++)
                {
                    var newProperty = new Property
                    {
                        AllowedPersons = property.AllowedPersons,
                        CheckInTime = property.CheckInTime,
                        CheckOutTime = property.CheckOutTime,
                        CityId = property.CityId,
                        CleanCondition = property.CleanCondition,
                        CreatedDate = property.CreatedDate,
                        DayPrice = property.DayPrice,
                        Count = property.Count,
                        DepositAmount = property.DepositAmount,
                        Description = property.Description,
                        Direction = property.Direction,
                        EnterTime = property.EnterTime,
                        ExitTime = property.ExitTime,
                        FamilyCondition = property.FamilyCondition,
                        InsuranceAmount = property.InsuranceAmount,
                        InsuranceCondition = property.InsuranceCondition,
                        IsDayPrice = property.IsDayPrice,
                        Latitude = property.Latitude,
                        Location = property.Location,
                        Longitude = property.Longitude,
                        MaximumAllowed = property.MaximumAllowed,
                        MoneyTransferCondition = property.MoneyTransferCondition,
                        OtherCondition = property.OtherCondition,
                        MoreThanAllowed = property.MoreThanAllowed,
                        MoreThanAllowedPrice = property.MoreThanAllowedPrice,
                        PropertyCode = property.PropertyCode,
                        PropertyName = property.PropertyName,
                        PropertyType = property.PropertyType,
                        ResortId = property.ResortId,
                        Region = property.Region,
                        Neighborhood = property.Neighborhood,
                        OrginalId= property.OrginalId,
                        Space= property.Space,
                        ViewStatus= property.ViewStatus,
                    };
                    var id = Guid.NewGuid();
                    newProperty.Id = id;
                    newProperty.OrginalId = property.Id;
                    newProperty.PropertyName += (i + 1);
                    _context.Add(newProperty);
                    for (int x = 0; x < property.PropertyImages.Count; x++)
                    {
                        var newImage = new PropertyImage();
                        newImage.PropertyId = id;
                        newImage.FileId = Domain.File.CopyImage(_context, property.PropertyImages.ToList()[x].File);
                        _context.Add(newImage);
                    }

                    foreach (var param in property.ParameterValues)
                    {
                        var newParam = new ParameterValue();
                        newParam.ParameterId = param.ParameterId;
                        newParam.PropertyId = id;
                        _context.Add(newParam);
                    }

                    if (property.PricePerDays!=null)
                    {
                        var dayPer = new PricePerDay {
                        Friday= property.PricePerDays.Friday,
                        Monday= property.PricePerDays.Monday,
                        PropertyId=id,
                        Saturday= property.PricePerDays.Saturday,
                        Sunday= property.PricePerDays.Sunday,
                        Thursday= property.PricePerDays.Thursday,
                        Tuesday= property.PricePerDays.Tuesday,
                        Wednesday= property.PricePerDays.Wednesday
                        };
                        _context.Add(dayPer);
                    }

                    foreach (var user in property.PropertyUsers)
                    {
                        var newUser = user;
                        newUser.Id = Guid.NewGuid();
                        newUser.PropertyId = id;
                        _context.Add(newUser);
                    }
                    foreach (var term in property.PropertyBookingTerms)
                    {
                        var t = new PropertyBookingTerm
                        {
                            BookingTermId = term.BookingTermId,
                            PropertyId = id
                        };
                        _context.Add(t);
                    }

                    foreach (var policy in property.PropertyCancellationPolicies)
                    {
                        var t = new PropertyCancellationPolicy
                        {
                            CancellationPolicyId = policy.CancellationPolicyId,
                            PropertyId = id
                        };
                        _context.Add(t);
                    }
                }
                _context.SaveChanges();
                trans.Commit();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Property), new { id = propertyId });
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw;
            }
        }

        [HttpPost]
        public IActionResult AddPropertyImage(Guid propertyId)
        {
            try
            {
                var isHasImages = _context.PropertyImages.Any(c => c.PropertyId == propertyId);
                var files = Request.Form.Files;
                var filesId = Domain.File.Upload("PropertiesImages", _context, files);
                foreach (var id in filesId)
                {
                    var propertyImage = new PropertyImage();
                    propertyImage.PropertyId = propertyId;
                    propertyImage.FileId = id;
                    if (!isHasImages)
                    {
                        isHasImages = true;
                        if (filesId.IndexOf(id) == 0)
                        {
                            propertyImage.IsPrimary = true;
                        }
                    }
                    _context.PropertyImages.Add(propertyImage);
                }
                var ActionName = "";
                var property = _context.Properties.Find(propertyId);
                if (property.CompleteStep==(int)enums.PropertySteps.ContactInfo)
                {
                    ActionName = "PropertyImages";
                }
                else
                {
                    ActionName = "PropertyUsers";
                }
                if (property.CompleteStep<(int)enums.PropertySteps.Images)
                {
                    property.CompleteStep = (int)enums.PropertySteps.Images;
                    _context.Update(property);
                    _context.SaveChanges();
                }
                Success(lang.AlertDataSavedSuccessfully);


                return RedirectToAction(ActionName, new { id = propertyId });
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        [HttpPost]
        public IActionResult RemovePropertyImage(Guid id, Guid propertyId)
        {
            Domain.File.Remove(_context, id);
            var propertyImage = _context.PropertyImages.FirstOrDefault(c => c.FileId == id);
            if (propertyImage != null)
            {
                _context.PropertyImages.Remove(propertyImage);
            }
            _context.SaveChanges();
            Success(lang.AlertDataSavedSuccessfully);
            return RedirectToAction("PropertyImages", new { id = propertyId });
        }

        [HttpPost]
        public IActionResult SetDefaultPropertyImage(Guid id, Guid propertyId)
        {
            var trans = _context.Database.BeginTransaction();
            //Domain.File.Remove(_context, id);
            var propertyImage = _context.PropertyImages.FirstOrDefault(c => c.FileId == id);
            if (propertyImage != null)
            {
                var defaultImage = _context.PropertyImages.FirstOrDefault(c => c.PropertyId == propertyId && c.IsPrimary);
                if (defaultImage != null)
                {
                    defaultImage.IsPrimary = false;
                    _context.Update(defaultImage);
                }

                propertyImage.IsPrimary = true;
                _context.Update(propertyImage);
            }
            _context.SaveChanges();
            trans.Commit();
            Success(lang.AlertDataSavedSuccessfully);
            return RedirectToAction("PropertyImages", new { id = propertyId });
        }

        [HttpPost]
        public IActionResult SavePropertyUtilities(Guid PropertyId, string[] inputValues, Guid[] inputIds, Guid[] checkboxes)
        {
            try
            {
                BindPropertyParameter(inputValues, inputIds, checkboxes, PropertyId);
                var property = _context.Properties.Find(PropertyId);
                var ActionName = "";
                if (property.CompleteStep == (int)enums.PropertySteps.ContactInfo)
                {
                    ActionName = "PropertyUtilities";
                }
                else
                {
                    ActionName = "PropertyImages";
                }
                if (property.CompleteStep<(int)enums.PropertySteps.Services)
                {
                    property.CompleteStep = (int)enums.PropertySteps.Services;
                    _context.Update(property);
                    _context.SaveChanges();
                }
                
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(ActionName, new { id = PropertyId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private void BindPropertyParameter(IReadOnlyList<string> inputValues, IReadOnlyList<Guid> inputIds, IEnumerable<Guid> checkboxes, Guid PropertyId)
        {
            try
            {
                var trans = _context.Database.BeginTransaction();
                var parametersValues = _context.ParameterValues.Where(c => c.PropertyId == PropertyId).ToList();
                _context.ParameterValues.RemoveRange(parametersValues);

                for (var i = 0; i < inputIds.Count; i++)
                {
                    var paramValues = new ParameterValue
                    {
                        PropertyId = PropertyId,
                        ParameterId = inputIds[i],
                        Value = inputValues[i]
                    };
                    _context.ParameterValues.Add(paramValues);
                }

                foreach (var checkbox in checkboxes)
                {
                    var paramValues = new ParameterValue
                    {
                        PropertyId = PropertyId,
                        ParameterId = checkbox,
                        Value = "1"
                    };
                    _context.ParameterValues.Add(paramValues);
                }
                _context.SaveChanges();
                trans.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IActionResult GetPropertyUtilities(Guid propertyId, int propertyType)
        {
            var model = new PropertyParameterViewModel();
            model.ParameterGroups = _context.ParameterGroups.Where(c => c.PropertyType == propertyType || c.PropertyType == (int)enums.PropertyType.All).Include(c => c.Parameters).ToList();
            model.ParameterValues = _context.ParameterValues.Where(c => c.PropertyId == propertyId).ToList();
            return PartialView("_propertyUtilities", model);
        }

        [HttpPost]
        public IActionResult AddOffer(Offer model)
        {
            try
            {
                _context.Offers.Add(model);
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction("Offers", new { id = model.PropertyId });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet]
        public IActionResult RemoveOffer(Guid id, Guid propertyId)
        {
            try
            {
                var offer = _context.Offers.Find(id);
                _context.Offers.Remove(offer);
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction("Offers", new { id = propertyId });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost]
        public IActionResult SaveUserInfo(User model)
        {
            try
            {
                var user = _context.Users.Find(model.Id);
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.BirthDate = model.BirthDate;
                _context.Users.Update(user);
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePropertyUser(User user, string PhoneNumberKey, string WhatsAppNumberKey, Guid PropertyId, bool WhatsAppNotifications)
        {
            var trans = _context.Database.BeginTransaction();
            try
            {
                var userInDb = _context.Users.Find(user.Id);
                if (userInDb == null)
                {
                    user.UserName = PhoneNumberKey + user.PhoneNumber;
                    user.PhoneNumber = PhoneNumberKey + user.PhoneNumber;
                    user.WhatsAppNumber = WhatsAppNumberKey + user.WhatsAppNumber;
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        var propertyUser = new PropertyUser
                        {
                            IsAdmin = user.UserType == (int)enums.UserType.BookAdmin ? true : false,
                            PropertyId = PropertyId,
                            UserId = user.Id,
                            SendWhatsAppNotifications = WhatsAppNotifications
                        };
                        _context.Add(propertyUser);
                        _context.SaveChanges();
                    }
                }
                else
                {
                    var phone = user.PhoneNumber.Remove(0, 4);
                    var whatsapp = user.WhatsAppNumber.Remove(0, 4);
                    userInDb.UserType = user.UserType;
                    user.PhoneNumber = PhoneNumberKey + phone;
                    user.WhatsAppNumber = WhatsAppNumberKey + whatsapp;
                    userInDb.FirstName = user.FirstName;
                    userInDb.LastName = user.LastName;
                    userInDb.Email = user.Email;
                    userInDb.Password = user.Password;
                    _context.Update(userInDb);
                    var propertyUser = _context.PropertyUsers.FirstOrDefault(c => c.UserId == userInDb.Id);
                    if (propertyUser != null)
                    {
                        propertyUser.IsAdmin = user.UserType == (int)enums.UserType.BookAdmin ? true : false;
                        propertyUser.SendWhatsAppNotifications = WhatsAppNotifications;
                        _context.PropertyUsers.Update(propertyUser);
                    }
                }
                var property = _context.Properties.Find(PropertyId);
                property.CompleteStep = (int)enums.PropertySteps.ContactInfo;
                _context.Update(property);
                _context.SaveChanges();
                trans.Commit();
                Success(lang.AlertDataSavedSuccessfully);
            }
            catch (Exception ex)
            {
                throw;
            }

            return RedirectToAction("PropertyUsers", new { id = PropertyId });
        }

        public IActionResult GetReservationCalender(Guid propertyId, int year, int month)
        {
            try
            {
                var days = DateTime.DaysInMonth(year, month);
                var data = new List<CalenderJson>();
                for (var i = 1; i < days + 1; i++)
                {
                    var date = new DateTime(year, month, i);

                    if (date.Date < DateTime.Now.Date)
                    {
                        data.Add(new CalenderJson
                        {
                            Date = date.ToString("MM-dd-yyy"),
                            Description = $"<button type='button' class='btn btn-secondary btn-sm disabled'>{lang.Select}</button>"
                        });
                        continue;
                    }
                    var reservation = _context.Reservations.Any(c =>
                        c.PropertyId == propertyId && (c.DateFrom.Date <= date.Date) &&
                        (c.DateTo.Date >= date.Date));
                    data.Add(reservation
                        ? new CalenderJson
                        {
                            Date = date.ToString("MM-dd-yyy"),
                            Description = $"<button type='button' class='btn btn-danger btn-sm'>{lang.PropertyReserved}</button>"
                        }
                        : new CalenderJson
                        {
                            Date = date.ToString("MM-dd-yyy"),
                            Description = $"<button type='button' onclick='selectDate(`{date.ToString("MM-dd-yyy")}`,`{date.AddDays(1).ToString("MM-dd-yyy")}`)' class='btn btn-success btn-sm'>{lang.Select}</button>"
                        });
                }
                //<button>Reserve</button>
                return Json(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public IActionResult IsReserved(Guid propertyId, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                ReservationViewModel model = new ReservationViewModel();
                Domain.Reservation.SetCheckedDates(HttpContext, checkIn, checkOut);
                var dates = Domain.Reservation.GetCheckedDates(HttpContext);
                model.Property = _context.Properties.Find(propertyId);
                Domain.Reservation.Reserved(_context, HttpContext, model.Property);
                model.offers = Domain.Reservation.getPropertyOffer(_context, model.Property, dates.CheckIn, dates.CheckOut);
                model.DiscountFromOffers = model.offers.Sum(c => c.Amount);
                model.TotalReservationPrice = Domain.Reservation.getReservationPrice(_context, model.Property, dates.CheckIn, dates.CheckOut);
                model.TotalDays = (dates.CheckOut - dates.CheckIn).Days;
                model.DateFrom = dates.CheckIn;
                model.DateTo = dates.CheckOut;
                return PartialView("_reservationForm", model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost]
        public IActionResult AddReservation(ReservationViewModel model, double PaidAmount)
        {
            var trans = _context.Database.BeginTransaction();
            try
            {
                var user = _context.Users.FirstOrDefault(c => c.PhoneNumber == model.User.PhoneNumber && c.FirstName == model.User.FirstName);
                if (user == null)
                {
                    user = new User
                    {
                        PhoneNumber = model.User.PhoneNumber,
                        FirstName = model.User.FirstName,
                        LastName = model.User.LastName,
                        Email = "",
                        IsAdmin = false,
                        UserType = (int)enums.UserType.EndUser
                    };
                    _context.Users.Add(user);
                    _context.SaveChanges();
                }
                var entryUser = _userManager.GetUserAsync(User).Result;
                var Property = _context.Properties.Find(model.Reservation.PropertyId);
                Domain.Reservation.Reserved(_context, HttpContext, Property);
                if (Property.IsReserved)
                {
                    Error(lang.PropertyReserved);
                    return RedirectToAction("Property", new { id = Property.Id });
                }
                model.Reservation.Status = (int)enums.ReservationStatus.New;
                model.Reservation.UserId = user.Id;
                model.Reservation.ReservedBy = (int)enums.ReservedBy.ProprityUser;
                model.Reservation.ReservedByUser = entryUser.FirstName + " " + entryUser.LastName;
                model.Reservation.ReservationNumber = 1;
                var propReservation = _context.Reservations.Where(c => c.PropertyId == model.Reservation.PropertyId).ToList();
                if (propReservation.Any())
                {
                    model.Reservation.ReservationNumber = propReservation.LastOrDefault().ReservationNumber + 1;
                }
                _context.Add(model.Reservation);
                _context.SaveChanges();

                if (PaidAmount > 0)
                {
                    payment(_context, model.Reservation, PaidAmount);
                }
                _context.SaveChanges();
                trans.Commit();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction("Reservations", new { id = Property.Id });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost]
        public IActionResult Payment(Guid ReservationId, double Amount)
        {
            try
            {
                var reservation = _context.Reservations.Include(c => c.Property).FirstOrDefault(c => c.Id == ReservationId);
                payment(_context, reservation, Amount);
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction("Reservations", new { id = reservation.PropertyId });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void payment(ApplicationDbContext context, Reservation reservation, double amount)
        {
            try
            {
                reservation.Status = (int)enums.ReservationStatus.Confirmed;
                context.Reservations.Update(reservation);
                context.Entry(reservation).Property("ReservationNumber").IsModified = false;
                context.SaveChanges();
                var user = _userManager.GetUserAsync(User).Result;
                Domain.Payment.PayCash(context, HttpContext, amount, reservation.Id, user.Id);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [AllowAnonymous]
        public IActionResult Invoice(Guid id)
        {
            try
            {
                var invoice = _context.PaymentTransactions.Find(id);
                if (invoice == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                var subPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices", invoice.ReservationId + "");
                if (!Directory.Exists(subPath))
                {
                    Domain.Payment.SaveInvoice(_context, HttpContext, invoice.ReservationId, id);
                }
                var pdfpath = Path.Combine(subPath, invoice.Id + ".pdf");
                if (!System.IO.File.Exists(pdfpath))
                {
                    Domain.Payment.SaveInvoice(_context, HttpContext, invoice.ReservationId, id);
                }
                var path = Url.Content("~/Invoices/" + invoice.ReservationId + "/" + invoice.Id + ".pdf");
                var stream = System.IO.File.OpenRead(pdfpath);
                return new FileStreamResult(stream, "application/pdf");
                //return File(path, "application/pdf","Invoice-"+invoice.RefNo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public IActionResult Details(Guid id)
        {
            try
            {
                var model = _context.Reservations.Include(c => c.Property.City).Include(c => c.Property.PropertyImages).ThenInclude(c => c.File).Include(c => c.Invoices).Include(c => c.User).FirstOrDefault(c => c.Id == id);
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult CancelReservation(Guid ReservationId, string CancelResoun)
        {

            try
            {
                var reservation = _context.Reservations.Find(ReservationId);
                if (reservation != null)
                {
                    reservation.Status = (int)enums.ReservationStatus.Cancled;
                    reservation.CancelResones = CancelResoun;
                    //_context.Reservations.Update(reservation);
                    _context.Entry(reservation).State = EntityState.Modified;
                    _context.Entry(reservation).Property("ReservationNumber").IsModified = false;
                    _context.SaveChanges();
                }
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction("Details", new { id = ReservationId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult AddInvoice(Guid ReservationId, double Amount)
        {
            var trans = _context.Database.BeginTransaction();
            try
            {
                var reservation = _context.Reservations.Find(ReservationId);
                if (reservation != null)
                {
                    reservation.Status = (int)enums.ReservationStatus.Confirmed;
                    _context.Entry(reservation).State = EntityState.Modified;
                    _context.SaveChanges();
                    var user = _userManager.GetUserAsync(User).Result;
                    Domain.Payment.PayCash(_context, HttpContext, Amount, ReservationId, user.Id);
                }
                Success(lang.AlertDataSavedSuccessfully);
                trans.Commit();
                return RedirectToAction("Details", new { id = ReservationId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IActionResult PropertyUtilities(Guid id)
        {
            try
            {
                var property = _context.Properties.Find(id);
                return View(property);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IActionResult Offers(Guid id)
        {
            try
            {
                var property = _context.Properties.Include(c => c.Offers).FirstOrDefault(c => c.Id == id);
                return View(property);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult PropertyUsers(Guid id)
        {
            try
            {
                var property = _context.Properties.Include(c => c.PropertyUsers).ThenInclude(c => c.User).FirstOrDefault(c => c.Id == id);
                return View(property);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IActionResult Reservations(Guid id)
        {
            try
            {
                var property = _context.Properties.Include(c => c.Reservations).FirstOrDefault(c => c.Id == id);
                return View(property);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IActionResult PropertyImages(Guid id)
        {
            try
            {
                var property = _context.Properties.Include(c => c.PropertyImages).ThenInclude(c => c.File).FirstOrDefault(c => c.Id == id);
                return View(property);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public IActionResult ResortUtilities(Guid id)
        {
            try
            {
                var property = _context.Resorts.Find(id);
                return View(property);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IActionResult ResortImages(Guid id)
        {
            try
            {
                var property = _context.Resorts.Include(c => c.ResortImages).ThenInclude(c => c.File).FirstOrDefault(c => c.Id == id);
                return View(property);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IActionResult ResortProperties(Guid id)
        {
            try
            {
                ViewBag.Resort = _context.Resorts.FirstOrDefault(c => c.Id == id);
                var properties = _context.Properties.Where(c => c.ResortId == id && !c.IsDeleted).ToList();
                return View(properties);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IActionResult RemoveResort(Guid id)
        {
            try
            {
                var resort = _context.Resorts.Find(id);
                if (resort != null)
                {
                    resort.IsDeleted = true;
                    resort.ViewStatus = false;
                    _context.Update(resort);
                    _context.SaveChanges();
                }
                return RedirectToAction(nameof(Properties));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IActionResult RemoveResortProperty(Guid id, Guid ResortId)
        {
            try
            {
                var property = _context.Properties.Find(id);
                if (property != null)
                {
                    property.IsDeleted = true;
                    property.ViewStatus = false;
                    _context.Update(property);
                    _context.SaveChanges();
                }
                return RedirectToAction(nameof(ResortProperties), new { id = ResortId });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IActionResult RemoveProperty(Guid id)
        {
            try
            {
                var property = _context.Properties.Find(id);
                if (property != null)
                {
                    property.IsDeleted = true;
                    property.ViewStatus = false;
                    _context.Update(property);
                    _context.SaveChanges();
                }
                return RedirectToAction(nameof(Properties));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    public IActionResult RemoveSimilerProperty(Guid id,Guid propertyId)
        {
            try
            {
                var property = _context.Properties.Find(id);
                if (property != null)
                {
                    property.IsDeleted = true;
                    property.ViewStatus = false;
                    _context.Update(property);
                    _context.SaveChanges();
                }
                return RedirectToAction(nameof(Property),new { id= propertyId });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IActionResult ResortReservations(Guid id)
        {
            try
            {
                ViewBag.Resort = _context.Resorts.Find(id);
                ViewBag.Properties = new SelectList(_context.Properties.Where(c => c.ResortId == id).ToList(), "Id", "PropertyName");
                List<Reservation> model = new();
                model = _context.Reservations.Where(c => c.Property.ResortId == id).ToList();
                return View(model);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private class CalenderJson
        {
            public string Date { get; set; }
            public string Description { get; set; }
        }

    }
}
