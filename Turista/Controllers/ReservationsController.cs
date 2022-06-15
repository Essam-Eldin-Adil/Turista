using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class ReservationsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _manager;
        public ReservationsController(ApplicationDbContext context, UserManager<User> manager)
        {
            _context = context;
            _manager = manager;
        }
        public IActionResult MyReservations()
        {
            try
            {
                var user = _manager.GetUserAsync(User).Result;
                ReservationsViewModel model = new ReservationsViewModel
                {
                    Reservations = _context.Reservations.Include(c => c.Property.City).Include(c=>c.Property.PropertyImages).ThenInclude(c=>c.File).Where(c => c.UserId == user.Id).ToList()
                };
                foreach (var reservation in model.Reservations)
                {
                    reservation.Property.IsFive = _context.Fiverates.Any(c=>c.PropertyId==reservation.PropertyId&&
                    c.UserId==user.Id);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        public IActionResult Details(Guid id)
        {
            try
            {
                var model = _context.Reservations.Include(c => c.Property.City).Include(c=>c.Property.PropertyImages).ThenInclude(c=>c.File).Include(c=>c.Invoices).Include(c=>c.User).FirstOrDefault(c => c.Id == id);
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
                return RedirectToAction("Details",new { id = ReservationId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
