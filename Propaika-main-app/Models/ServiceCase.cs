using System.ComponentModel.DataAnnotations.Schema;

namespace Propaika_main_app.Models
{
    public class ServiceCase
    {
        public int Id { get; set; }
        public required string Title { get; set; } 
        public string? Description { get; set; }
        public string? BeforeImage { get; set; } 
        public string? AfterImage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        public DateTime DateCompleted { get; set; } = DateTime.UtcNow;
    }
}
