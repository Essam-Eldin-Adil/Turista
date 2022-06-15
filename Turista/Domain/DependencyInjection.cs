using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Turista.Data;

namespace Domain
{
    public static class DependencyInjection
    {
        public static void ServicesRegister(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IDataContext), typeof(ApplicationDbContext));


        }
    }
}
