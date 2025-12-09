using System.ComponentModel.DataAnnotations.Schema;

namespace Propaika_main_app.Models
{
    public class ServiceItem
    {
        public int Id { get; set; }

        public required string ServiceName { get; set; }
        public string? Description { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; } 

        public bool IsPopular { get; set; } 

        public int DeviceModelId { get; set; }
        public List<DeviceModelServiceItem> DeviceModelServices { get; set; } = new();
    }
}
