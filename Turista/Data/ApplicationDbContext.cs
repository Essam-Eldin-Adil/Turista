using iQuarc.DataLocalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Turista.Data.Identity;
using Turista.Data.General;
using Turista.Data.Properties.PropertyDetails;
using Turista.Data.Properties;
using Turista.Data.Resorts.PropertyDetails;
using Turista.Data.Reservation;

namespace Turista.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IDataContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            LocalizationConfig.RegisterLocalizationEntity<Language>(l => l.Code);
            LocalizationConfig.RegisterCultureMapper(c => c.LCID);
        }


        private IDbContextTransaction _transaction;

        /// <summary>
        /// Convegrations
        /// </summary>
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<LanguageTranslation> LanguageTranslations { get; set; }

        /// <summary>
        /// Identity
        /// </summary>

        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<UserPermission> UserPermissions { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public virtual DbSet<AppSetting> AppSettings { get; set; }
        public virtual DbSet<ParameterGroup> ParameterGroups { get; set; }
        public virtual DbSet<ParameterGroupTranslation> ParameterGroupTranslations { get; set; }
        public virtual DbSet<Parameter> Parameters { get; set; }
        public virtual DbSet<ParameterTranslation> ParameterTranslations { get; set; }
        public virtual DbSet<ParameterValue> ParameterValues { get; set; }
        public virtual DbSet<Property> Properties { get; set; }
        public virtual DbSet<PropertyImage> PropertyImages { get; set; }
        public virtual DbSet<PropertyUser> PropertyUsers { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Fiverate> Fiverates { get; set; }
        public virtual DbSet<Reservation.Reservation> Reservations { get; set; }
        public virtual DbSet<Reservation.PaymentTransaction> PaymentTransactions { get; set; }
        public virtual DbSet<Resorts.Resort> Resorts { get; set; }
        public virtual DbSet<Resorts.ResortImage> ResortImages { get; set; }
        public virtual DbSet<Resorts.ResortUser> ResortUsers { get; set; }
        public virtual DbSet<ResortParameterValue> ResortParameterValues { get; set; }
        public virtual DbSet<Offer> Offers { get; set; }
        public virtual DbSet<PricePerDay> PricePerDays { get; set; }
        public virtual DbSet<ContactUs> ContactsUs { get; set; }
        public virtual DbSet<BookingTerm> BookingTerms { get; set; }
        public virtual DbSet<CancellationPolicy> CancellationPolicies { get; set; }
        public virtual DbSet<PropertyBookingTerm> PropertyBookingTerms { get; set; }
        public virtual DbSet<PropertyCancellationPolicy> PropertyCancellationPolicies { get; set; }

        /// <summary>
        /// General
        /// </summary>
        public DbSet<File> Files { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<UserRole>().ToTable("UserRoles");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            //builder.Entity<User>(user => 
            //{
            //    user.Property(u => u.UserName).
            //});
            builder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.UserId)
                  .IsRequired();

            });
        }
        internal Task AddAsync(Func<IActionResult> rolePermissions)
        {
            throw new NotImplementedException();
        }

        public void BeginTransaction()
        {
            _transaction = Database.BeginTransaction();
        }

        public void Commit()
        {
            try
            {
                SaveChanges();
                _transaction.Commit();
            }
            catch
            {
                Rollback();
            }
            finally
            {
                _transaction.Dispose();
            }
        }

        internal Task DeleteAsync<T>(T model)
        {
            throw new NotImplementedException();
        }

        internal Task DeleteAsync<T>(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }
        public void Migrate()
        {
            Database.Migrate();
        }
        // public DbSet<RequestQuotation> RequestQuotations { get; set; }
        //public DbSet<RequestQuotationItem> RequestQuotationItems { get; set; }

    }
}
