using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data;
using Turista.Data.Identity;
using Turista.Data.Properties;
using Turista.Data.Resorts;
using Turista.Models;
using Turista.Resources;

namespace Turista.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class CPanelController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public CPanelController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Users
        /// </summary>
        /// <returns></returns>
        public IActionResult Properties()
        {
            return View();
        }

        public ActionResult<IEnumerable<Property>> GetPropertiesData()
        {
            try
            {
                var query = Request.Query;
                var take = 0;
                var skip = 0;
                int.TryParse(query["length"], out take);
                int.TryParse(query["start"], out skip);
                var search = query["search[value]"][0];
                var draw = query["draw"];
                var data = _context.Properties.Include(c => c.City).Where(c => (string.IsNullOrEmpty(search) || c.PropertyName.Contains(search)) || (string.IsNullOrEmpty(search) || c.City.CityName.Contains(search) || (string.IsNullOrEmpty(search) || c.PropertyUsers.Select(c => c.User.PhoneNumber).Contains(search)))).OrderBy(a => a.CreatedDate).Skip(skip).Take(take).ToList();
                return Ok(new { draw = draw, data, recordsTotal = _context.Properties.Count(), recordsFiltered = _context.Properties.Count() });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult<IEnumerable<Resort>> GetResortsData()
        {
            try
            {
                var query = Request.Query;
                var take = 0;
                var skip = 0;
                int.TryParse(query["length"], out take);
                int.TryParse(query["start"], out skip);
                var search = query["search[value]"][0];
                var draw = query["draw"];
                var data = _context.Resorts.Include(c => c.City).Where(c => (string.IsNullOrEmpty(search) || c.Name.Contains(search)) || (string.IsNullOrEmpty(search) || c.City.CityName.Contains(search) || (string.IsNullOrEmpty(search) || c.ResortUsers.Select(c => c.User.PhoneNumber).Contains(search)))).OrderBy(a => a.CreatedDate).Skip(skip).Take(take).ToList();
                return Ok(new { draw = draw, data, recordsTotal = _context.Resorts.Count(), recordsFiltered = _context.Resorts.Count() });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IActionResult ConfirmProperty(Guid id, bool confirm)
        {
            try
            {
                var chalet = _context.Properties.Find(id);
                chalet.IsConfirmed = confirm;
                _context.Properties.Update(chalet);
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Properties));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public IActionResult ConfirmResort(Guid id, bool confirm)
        {
            try
            {
                var chalet = _context.Resorts.Find(id);
                chalet.IsConfirmed = confirm;
                _context.Resorts.Update(chalet);
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Properties));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        //////////
        ///Cities
        ///
        public IActionResult Cities()
        {
            return View();
        }

        public ActionResult<IEnumerable<City>> GetCitiesData()
        {
            try
            {
                var query = Request.Query;
                int take = 0;
                int skip = 0;
                int userType = 0;
                int.TryParse(query["length"], out take);
                int.TryParse(query["start"], out skip);
                var search = query["search[value]"][0];
                var draw = query["draw"];
                var data = _context.Cities.Where(c => (string.IsNullOrEmpty(search) || c.CityName == search)).OrderBy(a => a.CreatedDate).Skip(skip).Take(take).ToList();
                foreach (var city in data)
                {
                    city.ImageUrl = Url.Content("~/" + Domain.File.GetImage(_context, city.Image));
                }
                return Ok(new { draw = draw, data, recordsTotal = _context.Cities.Count(), recordsFiltered = _context.Cities.Count() });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IActionResult City(Guid? id)
        {
            var model = new CityViewModel();
            try
            {
                if (id != null)
                {
                    model.City = _context.Cities.FirstOrDefault(c => c.Id == id);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveCity(CityViewModel model)
        {
            try
            {
                var city = _context.Cities.Find(model.City.Id);
                if (city == null)
                {
                    city = new City { CityName = model.City.CityName};
                    _context.Cities.Add(city);
                }
                else
                {
                    city.CityName = model.City.CityName;
                    _context.Cities.Update(city);
                }
                var files = Request.Form.Files;
                if (files.Count > 0)
                {
                    if (city.Image != Guid.Empty && city.Image != null)
                    {
                        Domain.File.Remove(_context, (Guid)city.Image);
                    }
                    var filesId = Domain.File.Upload("CitiesImages", _context, files);
                    foreach (var guid in filesId)
                    {
                        city.Image = guid;
                        _context.Cities.Update(city);
                    }
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(City), new { id = city.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


        /// <summary>
        /// Users
        /// </summary>
        /// <returns></returns>
        public IActionResult Users()
        {
            return View(new User());
        }

        public ActionResult<IEnumerable<User>> GetData()
        {
            try
            {
                var query = Request.Query;
                int take = 0;
                int skip = 0;
                int userType = 0;
                int.TryParse(query["length"], out take);
                int.TryParse(query["start"], out skip);
                int.TryParse(query["userType"], out userType);
                var search = query["search[value]"][0];
                var draw = query["draw"];
                var data = _context.Users.Where(c => c.UserType == userType && (string.IsNullOrEmpty(search) || c.Email == search
                                                                                         || c.FirstName == search ||
                                                            c.LastName == search || c.WhatsAppNumber == search
                                                                                         || c.PhoneNumber == search)).OrderBy(a => a.CreatedDate).Skip(skip).Take(take).ToList(); ;
                return Ok(new { draw = draw, data, recordsTotal = _context.Users.Count(c => c.UserType == userType), recordsFiltered = _context.Users.Count(c => c.UserType == userType) });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost]
        public IActionResult SaveUser(User model)
        {
            try
            {
                var user = _context.Users.Find(model.Id);
                if (user == null)
                {
                    user = new User();
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.WhatsAppNumber = model.WhatsAppNumber;
                    user.Email = model.Email;
                    user.BirthDate = model.BirthDate;
                    user.UserType = model.UserType;
                    _context.Users.Add(user);
                }
                else
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.WhatsAppNumber = model.WhatsAppNumber;
                    user.Email = model.Email;
                    user.BirthDate = model.BirthDate;
                    user.UserType = model.UserType;
                    _context.Users.Update(user);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public IActionResult RemoveUser(Guid id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user != null)
                {
                    _context.Remove(user);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


        /////////
        ///Utit
        ///

        public IActionResult Utilities()
        {
            UtilitiesViewModel model = new UtilitiesViewModel();
            try
            {
                ViewBag.Languages = new SelectList(_context.Languages.ToList(), "Id", "Name");

                var param = _context.ParameterGroups.Include("Parameters.ParameterTranslations").Include(c => c.ParameterGroupTranslations).Where(c => c.ParentId == Guid.Empty).ToList();
                foreach (var parameterGroup in param)
                {
                    var itemTree = new ItemTree { ParameterGroup = parameterGroup };
                    var nodes = _context.ParameterGroups.Include("Parameters.ParameterTranslations").Include(c => c.ParameterGroupTranslations).Where(c => c.ParentId == parameterGroup.Id).ToList();
                    itemTree.Type = parameterGroup.PropertyType;
                    itemTree.HaveNodes = nodes.Any();
                    if (itemTree.HaveNodes)
                    {
                        itemTree.ParameterGroups = nodes;
                    }
                    model.ItemTree.Add(itemTree);
                }
                return View(model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost]
        public IActionResult SaveParameter(UtilitiesViewModel model)
        {
            try
            {
                var parameter = _context.Parameters.Find(model.Parameter.Id);
                if (parameter == null)
                {
                    _context.Parameters.Add(model.Parameter);
                }
                else
                {
                    parameter.Index = model.Parameter.Index;
                    parameter.Name = model.Parameter.Name;
                    parameter.Type = model.Parameter.Type;
                    parameter.Icon = model.Parameter.Icon;
                    _context.Parameters.Update(parameter);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Utilities));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        [HttpPost]
        public IActionResult SaveSubGroup(UtilitiesViewModel model)
        {
            try
            {
                var parameterGroup = _context.ParameterGroups.Find(model.ParameterGroup.Id);
                if (parameterGroup == null)
                {
                    if (model.ParameterGroup.ParentId != Guid.Empty)
                    {
                        model.ParameterGroup.IsChild = true;
                    }
                    _context.ParameterGroups.Add(model.ParameterGroup);
                }
                else
                {
                    parameterGroup.Filterable = model.ParameterGroup.Filterable;
                    parameterGroup.PropertyType = model.ParameterGroup.PropertyType;
                    parameterGroup.Name = model.ParameterGroup.Name;
                    parameterGroup.Order = model.ParameterGroup.Order;
                    parameterGroup.Icon = model.ParameterGroup.Icon;
                    _context.ParameterGroups.Update(parameterGroup);
                }
                

                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Utilities));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public IActionResult RemoveGroup(Guid id)
        {
            try
            {
                var group = _context.ParameterGroups.Find(id);
                if (group != null)
                {
                    var subs = _context.ParameterGroups.Where(c => c.ParentId == id);
                    _context.ParameterGroups.RemoveRange(subs);
                    _context.ParameterGroups.Remove(group);
                    
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Utilities));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public IActionResult RemoveParameter(Guid id)
        {
            try
            {
                var parameter = _context.Parameters.Find(id);
                if (parameter != null)
                {
                    _context.Parameters.Remove(parameter);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Utilities));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        [HttpPost]
        public IActionResult SaveGroupTranslation(UtilitiesViewModel model)
        {
            try
            {
                var groupTranslation = _context.ParameterGroupTranslations.Find(model.ParameterGroupTranslation.Id);
                if (groupTranslation == null)
                {
                    _context.ParameterGroupTranslations.Add(model.ParameterGroupTranslation);
                }
                else
                {
                    groupTranslation.Name = model.ParameterGroupTranslation.Name;
                    groupTranslation.LanguageId = model.ParameterGroupTranslation.LanguageId;
                    _context.ParameterGroupTranslations.Update(groupTranslation);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Utilities));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        [HttpPost]
        public IActionResult SaveParameterTranslation(UtilitiesViewModel model)
        {
            try
            {
                var parameterTranslation = _context.ParameterTranslations.Find(model.ParameterTranslation.Id);
                if (parameterTranslation == null)
                {
                    _context.ParameterTranslations.Add(model.ParameterTranslation);
                }
                else
                {
                    parameterTranslation.Name = model.ParameterTranslation.Name;
                    parameterTranslation.LanguageId = model.ParameterTranslation.LanguageId;
                    _context.ParameterTranslations.Update(parameterTranslation);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Utilities));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


        public IActionResult RemoveGroupTranslation(Guid id)
        {
            try
            {
                var groupTranslation = _context.ParameterGroupTranslations.Find(id);
                if (groupTranslation != null)
                {
                    _context.ParameterGroupTranslations.Remove(groupTranslation);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Utilities));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public IActionResult RemoveParameterTranslation(Guid id)
        {
            try
            {
                var parameterTranslation = _context.ParameterTranslations.Find(id);
                if (parameterTranslation != null)
                {
                    _context.ParameterTranslations.Remove(parameterTranslation);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Utilities));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public IActionResult Policies()
        {
            try
            {
                var model = new PoliciesViewModel() {
                    CancellationPolicies = _context.CancellationPolicies.ToList(),
                    BookingTerms = _context.BookingTerms.ToList()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        [HttpPost]
        public IActionResult SaveBookingTerm(PoliciesViewModel model)
        {
            try
            {
                var result = _context.BookingTerms.Any(c=>c.Id==model.BookingTerm.Id);
                if (!result)
                {
                    _context.BookingTerms.Add(model.BookingTerm);
                }
                else
                {
                    _context.BookingTerms.Update(model.BookingTerm);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Policies));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        [HttpPost]
        public IActionResult SaveCancellationPolicy(PoliciesViewModel model)
        {
            try
            {
                var result = _context.CancellationPolicies.Any(c=>c.Id==model.CancellationPolicy.Id);
                if (!result)
                {
                    _context.CancellationPolicies.Add(model.CancellationPolicy);
                }
                else
                {
                    _context.CancellationPolicies.Update(model.CancellationPolicy);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Policies));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public IActionResult RemoveCancellationPolicy(Guid id)
        {
            try
            {
                var cancellationPolicy = _context.CancellationPolicies.Find(id);
                if (cancellationPolicy != null)
                {
                    _context.CancellationPolicies.Remove(cancellationPolicy);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Policies));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public IActionResult RemoveBookingTerm(Guid id)
        {
            try
            {
                var bookingTerm = _context.BookingTerms.Find(id);
                if (bookingTerm != null)
                {
                    _context.BookingTerms.Remove(bookingTerm);
                }
                _context.SaveChanges();
                Success(lang.AlertDataSavedSuccessfully);
                return RedirectToAction(nameof(Policies));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


        public IActionResult Settings()
        {
            try
            {
                var settings = _context.AppSettings.FirstOrDefault();
                return View(settings);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult SaveSettings(Turista.Data.General.AppSetting appSetting)
        {
            try
            {
                if (_context.AppSettings.Any())
                {
                    _context.Update(appSetting);
                }
                else
                {
                    _context.Add(appSetting);
                }
                _context.SaveChanges();
                return View(appSetting);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}
