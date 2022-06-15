using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Turista.Data;
using Turista.Data.General;
using static Turista.Models.enums;

namespace EProcurementSolution.Helpers
{
    public static class Logger
    {
        public static void SystemLog(Exception ex,ApplicationDbContext _context)
        {
            var log = new Log();
            log.Type = (int)LogType.System;
            log.Message = ex.Message;
            log.Content = ex.StackTrace;
            _context.Logs.Add(log);
            _context.SaveChanges();
        }

        public static void DatabaseLog(string actionType, string action, ApplicationDbContext _context)
        {
            var log = new Log();
            log.Type = (int)LogType.Database;
            log.Message = actionType;
            log.Content = action;
            _context.Logs.Add(log);
            _context.SaveChanges();
        }

        public static void UserLog(ClaimsPrincipal user,string actionType, string action, ApplicationDbContext _context)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var log = new Log();
            log.Type = (int)LogType.User;
            log.Message = actionType;
            log.Content = action;
            log.UserId = Guid.Parse(userId);
            _context.Logs.Add(log);
            _context.SaveChanges();
        }
    }
}
