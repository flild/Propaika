using Propaika_main_app.Extra;
using System.ComponentModel.DataAnnotations;

namespace Propaika_main_app.Models
{
    public class RepairRequest
    {
        public int Id { get; set; }

        [Display(Name = "Ваше имя")]
        [Required(ErrorMessage = "Пожалуйста, представьтесь")]
        public string ClientName { get; set; } = string.Empty;

        [Display(Name = "Телефон")]
        [Required(ErrorMessage = "Телефон обязателен для связи")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Описание проблемы")]
        public string? ProblemDescription { get; set; }

        // Изменили на string для простоты ввода в форме
        [Display(Name = "Модель устройства")]
        public string? DeviceModel { get; set; }

        // Можно оставить пустым или использовать скрытое поле, если нужно
        public string? ServiceItem { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Статус заявки")]
        public RequestStatus Status { get; set; } = RequestStatus.New;
    }
}
