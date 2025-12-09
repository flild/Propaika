using System.ComponentModel.DataAnnotations.Schema;

namespace Propaika_main_app.Models
{
    public class DeviceModelServiceItem
    {
        public int DeviceModelId { get; set; }
        public DeviceModel DeviceModel { get; set; } = null!;
        public int ServiceItemId { get; set; }
        public ServiceItem ServiceItem { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}
