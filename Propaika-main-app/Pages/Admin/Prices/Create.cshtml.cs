using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Propaika_main_app.Data;
using Propaika_main_app.Models;
using System.ComponentModel.DataAnnotations;

namespace Propaika_main_app.Pages.Admin.Prices
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public CreateModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public SelectList DeviceModels { get; set; } = null!;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [Display(Name = "Модель устройства")]
            public int DeviceModelId { get; set; }

            [Required]
            [Display(Name = "Название услуги")]
            public string ServiceName { get; set; } = string.Empty;

            [Display(Name = "Описание")]
            public string? Description { get; set; }

            [Required]
            [Range(0, 1000000)]
            [Display(Name = "Цена, ₽")]
            public decimal Cost { get; set; }

            [Display(Name = "Популярная услуга (показывать на главной)")]
            public bool IsPopular { get; set; }
        }

        public async Task OnGetAsync()
        {
            DeviceModels = new SelectList(_db.DeviceModels.OrderBy(d => d.Name), "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            DeviceModels = new SelectList(_db.DeviceModels.OrderBy(d => d.Name), "Id", "Name");

            if (!ModelState.IsValid) return Page();

            var price = new ServiceItem
            {
                DeviceModelId = Input.DeviceModelId,
                ServiceName = Input.ServiceName,
                Description = Input.Description,
                Cost = Input.Cost,
                IsPopular = Input.IsPopular
            };

            _db.ServiceItems.Add(price);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Услуга добавлена в прайс.";
            return RedirectToPage("./Index", new { FilterDeviceId = Input.DeviceModelId });
        }
    }
}
