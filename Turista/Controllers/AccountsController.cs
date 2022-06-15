using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turista.Data;
using Turista.Data.Identity;
using Turista.Models;
using Turista.Resources;

namespace Turista.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountsController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public IActionResult Login(string ReturnUrl)
        {
            ViewBag.refererUrl = ReturnUrl;
            return View();
        }
        public IActionResult ClientLogin(string ReturnUrl)
        {
            ViewBag.refererUrl = ReturnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CheckPhoneNumber(string phone,int userType)
        {
            try
            {
                if (phone.Length<13|| phone.Length > 13)
                {
                    return PartialView("_error", lang.PhoneNumberLength);
                }
                var user = _context.Users.FirstOrDefault(c=>c.PhoneNumber.Equals(phone));
                if (user!=null)
                {
                    if (!user.PhoneNumberConfirmed)
                    {
                        SendOTP(user);
                        return PartialView("_otp", user);
                    }
                    return PartialView("_password", user);
                }
                else
                {
                    user = new User();
                    user.Id = Guid.NewGuid();
                    user.UserName = phone;
                    user.PhoneNumber = phone;
                    user.UserType = userType;
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        SendOTP(user);
                        return PartialView("_otp", user);
                    }
                    else
                    {
                        return PartialView("_ops");
                    }
                }
            }
            catch (Exception ex)
            {
                return PartialView("_error", ex.Message);
                throw;
            }
        }

        private void SendOTP(User user)
        {
            user.OTP = 123456;
            _context.Users.Update(user);
            _context.SaveChanges();
            //throw new NotImplementedException();
        }
        [HttpPost]
        public IActionResult CheckOtp(string phone, int otp)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(c => c.PhoneNumber.Equals(phone) && c.OTP.Equals(otp));
                if (user != null)
                {
                    return PartialView("_completeProfile", user);
                }
                else
                {
                    return PartialView("_error",lang.WrongOtp);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        public IActionResult ResendOtp(string phone)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(c => c.PhoneNumber.Equals(phone));
                if (user != null)
                {
                    SendOTP(user);
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [HttpPost]
        public IActionResult ForgetPassword(string phone)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(c => c.PhoneNumber.Equals(phone));
                if (user != null)
                {
                    user.PhoneNumberConfirmed = false;
                    SendOTP(user);
                    return PartialView("_otp", user);
                }
                else
                {
                    return PartialView("_error", lang.WrongPhoneNumber);
                }
            }
            catch (Exception ex)
            {
                return PartialView("_error", ex.Message);
                throw;
            }
        }


        [HttpPost]
        public async Task<IActionResult> CompleteProfile(Guid Id,string FirstName,string LastName, string WhatsAppNumber,string Password,string key,string SamePhoneNumber)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(Id.ToString());
                if (await _userManager.HasPasswordAsync(user))
                {
                    await _userManager.RemovePasswordAsync(user);
                }
                var result = await _userManager.AddPasswordAsync(user, Password);
                if (result.Succeeded)
                {
                    user.PhoneNumberConfirmed = true;
                    user.EmailConfirmed = true;
                    user.FirstName = FirstName;
                    user.LastName = LastName;
                    if (string.IsNullOrEmpty(SamePhoneNumber))
                    {
                        SamePhoneNumber = "";
                    }
                    if (SamePhoneNumber.Contains("on"))
                    {
                        user.WhatsAppNumber = user.PhoneNumber;
                    }
                    else
                    {
                        if(string.IsNullOrEmpty(user.WhatsAppNumber))
                            user.WhatsAppNumber = key+WhatsAppNumber;
                    }

                    _context.Update(user);
                    _context.SaveChanges();
                    if (user.UserType==(int)enums.UserType.BookAdmin)
                    {
                        await _userManager.AddToRoleAsync(user, "PropertyAdmin");
                    }
                    var signIn = await _signInManager.PasswordSignInAsync(user, Password,false, true);
                    if (signIn.Succeeded)
                    {
                        HttpContext.Session.SetString("UserId", user.Id.ToString());
                        HttpContext.Session.SetString("UserImage", user.Image.ToString());
                        return RedirectToAction("Redircet","Home");
                    }
                    else
                    {
                        return RedirectToAction("Error");
                    }
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckPassword(string phone,string password,bool rememberMe, int userType,int RoutingUserType)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(c => c.PhoneNumber.Equals(phone));
                var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, false);
                if (result.Succeeded)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("UserImage", user.Image.ToString());
                    if (RoutingUserType != user.UserType)
                    {
                        return Json("NotValidUser");
                    }
                    return Json("Success");
                }
                else
                {
                    return Json(lang.WrongPassword);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
