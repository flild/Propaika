using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Extra;
using Propaika_main_app.Models;

namespace Propaika_main_app.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DeviceModel> DeviceModels { get; set; }
        public DbSet<ServiceItem> ServiceItems { get; set; }
        public DbSet<ServiceCase> ServiceCases { get; set; }
        public DbSet<RepairRequest> RepairRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Сидирование начальных данных (чтобы база не была пустой)
            modelBuilder.Entity<DeviceModel>().HasData(
                new DeviceModel { Id = 1, Name = "iPhone 13", DeviceType = DeviceType.Phone },
                new DeviceModel { Id = 2, Name = "iPhone 14 Pro", DeviceType = DeviceType.Phone },
                new DeviceModel { Id = 3, Name = "MacBook Air M1", DeviceType = DeviceType.Phone }
            );
        }
    }
}
