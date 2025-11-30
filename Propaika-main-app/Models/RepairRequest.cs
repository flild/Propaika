namespace Propaika_main_app.Models
{
    public class RepairRequest
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; }
        public string? ProblemDescription { get; set; }
        public DeviceModel? DeviceModel { get; set; }
        public ServiceItem? ServiceItem { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsProcessed { get; set; } 
    }
}
