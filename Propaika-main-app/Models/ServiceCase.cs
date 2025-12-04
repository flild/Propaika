using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Propaika_main_app.Models
{
    public class ServiceCase
    {
        public int Id { get; set; }
        public string Title { get; set; } 
        public string? Description { get; set; }
        public string? BeforeImage { get; set; } 
        public string? AfterImage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        public DateTime DateCompleted { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string Slug { get; set; } = string.Empty; // ЧПУ для URL: /cases/remont-iphone-13

        [MaxLength(100)]
        public string? DeviceModel { get; set; } // Модель: "iPhone 13 Pro"

        [MaxLength(100)]
        public string? ServiceType { get; set; } // Тип работы: "Замена экрана"

        // SEO meta-теги
        [MaxLength(200)]
        public string? MetaTitle { get; set; }

        [MaxLength(300)]
        public string? MetaDescription { get; set; }
    }
}
