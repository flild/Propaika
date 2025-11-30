using Propaika_main_app.Extra;

namespace Propaika_main_app.Models
{
    public class DeviceModel
    {
        public int Id { get; set; }
        public required string Name { get; set; } 
        public DeviceType DeviceType { get; set; }

        // Связь с ценами для этого устройства
        public List<ServiceItem> ServiceItems { get; set; } = new();
    }
}
