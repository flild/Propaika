using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Propaika_main_app.Data;
using Propaika_main_app.Extra;
using Propaika_main_app.Models;
using System.ComponentModel.DataAnnotations;

namespace Propaika_main_app.Pages.Admin.Devices
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public CreateModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Укажите название модели")]
            [Display(Name = "Название модели")]
            public string Name { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Тип устройства")]
            public DeviceType DeviceType { get; set; } = DeviceType.Phone;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var device = new DeviceModel
            {
                Name = Input.Name,
                DeviceType = Input.DeviceType
            };

            _db.DeviceModels.Add(device);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Модель {device.Name} добавлена!";
            return RedirectToPage("./Index");
        }
    }
}
