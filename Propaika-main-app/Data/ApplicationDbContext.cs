using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Extra;
using Propaika_main_app.Models;

namespace Propaika_main_app.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DeviceModel> DeviceModels { get; set; }
        public DbSet<ServiceItem> ServiceItems { get; set; }
        public DbSet<ServiceCase> ServiceCases { get; set; }
        public DbSet<RepairRequest> RepairRequests { get; set; }
        public DbSet<DeviceModelServiceItem> DeviceModelServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ServiceCase>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            modelBuilder.Entity<DeviceModelServiceItem>()
    .HasKey(dms => new { dms.DeviceModelId, dms.ServiceItemId });

            modelBuilder.Entity<DeviceModelServiceItem>()
                .HasOne(dms => dms.DeviceModel)
                .WithMany(dm => dm.DeviceModelServices)
                .HasForeignKey(dms => dms.DeviceModelId);

            modelBuilder.Entity<DeviceModelServiceItem>()
                .HasOne(dms => dms.ServiceItem)
                .WithMany(si => si.DeviceModelServices)
                .HasForeignKey(dms => dms.ServiceItemId);
        }
    }
}
