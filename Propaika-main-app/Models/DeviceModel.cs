namespace Propaika_main_app.Models
{
    public class DeviceModel
    {
        public int Id { get; set; }
        public required string Name { get; set; } // Например: "iPhone 13 Pro"
        public string DeviceType { get; set; } = "Phone"; 

        // Связь с ценами для этого устройства
        public List<ServiceItem> ServiceItems { get; set; } = new();
    }
}
