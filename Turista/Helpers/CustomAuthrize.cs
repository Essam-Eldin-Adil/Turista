using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Turista.Data;
using Turista.Data.Identity;

namespace EProcurementSolution.Helpers
{
    //public class CustomAuthrize
    public class CustomAuthrize : TypeFilterAttribute
    {
        public CustomAuthrize(string permission) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { new CustomArg(permission) };
        }
    }

    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        readonly CustomArg _permission;
        readonly ApplicationDbContext _context;

        public ClaimRequirementFilter(CustomArg permission, ApplicationDbContext context)
        {
            _permission = permission;
            _context = context;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //This's to stor all permissions
            List<Permission> permissions = new List<Permission>();
            var userName = context.HttpContext.User.Identity.Name;
            var userId = _context.Users.FirstOrDefault(c => c.UserName == userName).Id;

            var userRoles = _context.UserRoles.Where(c => c.UserId.Equals(userId)).ToList();
            foreach (var item in userRoles)
            {
                var rolePermissions = _context.RolePermissions.Include(c => c.Permission).Where(c => c.RoleId == item.RoleId).ToList().Select(c=>c.Permission);
                permissions.AddRange(rolePermissions);
            }
            var userPermissions = _context.UserPermissions.Include(c=>c.Permission).Include(c => c.User).Where(c => c.User.UserName.Equals(userName)).ToList().Select(c=>c.Permission);
            permissions.AddRange(userPermissions);
            if (!permissions.Any(c=>c.Name.Equals(_permission.Permission)))
            {
                context.Result = new ForbidResult();
            }
        }
    }

    public class CustomArg {
        public CustomArg(string _permission)
        {
            Permission = _permission;
        }
        public string Permission { get; set; }
    }

   public static class Permissions
    {
        public static bool IsHavePermission(string userName, ApplicationDbContext _context, string permissionName)
        {
            var userId = _context.Users.FirstOrDefault(c => c.UserName.Equals(userName)).Id;
            List<Permission> permissions = new List<Permission>();
            var userRoles = _context.UserRoles.Where(c => c.UserId.Equals(userId)).ToList();
            foreach (var item in userRoles)
            {
                var rolePermissions = _context.RolePermissions.Include(c=>c.Permission).Where(c => c.RoleId == item.RoleId).ToList().Select(c => c.Permission);
                permissions.AddRange(rolePermissions);
            }
            var userPermissions = _context.UserPermissions.Include(c => c.Permission).Include(c => c.User).Where(c => c.UserId.Equals(userId)).ToList().Select(c => c.Permission);
            permissions.AddRange(userPermissions);
            return permissions.Any(c => c.Name.Equals(permissionName));
        }
    }
}
