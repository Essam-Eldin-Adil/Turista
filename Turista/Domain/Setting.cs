using System.Linq;
using Turista.Data;
using Turista.Data.General;

namespace Domain
{
    public static class Settings
    {
        public static AppSetting Get(ApplicationDbContext httpContext)
        {
            return httpContext.AppSettings.FirstOrDefault();
        }
    }
}
