using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using System.ComponentModel.DataAnnotations;

namespace Propaika_main_app.Pages.Admin.Prices
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public EditModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public SelectList DeviceModels { get; set; } = null!;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

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

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var price = await _db.ServiceItems.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (price == null) return NotFound();

            DeviceModels = new SelectList(await _db.DeviceModels.OrderBy(d => d.Name).ToListAsync(), "Id", "Name");

            Input = new InputModel
            {
                Id = price.Id,
                DeviceModelId = price.DeviceModelId,
                ServiceName = price.ServiceName,
                Description = price.Description,
                Cost = price.Cost,
                IsPopular = price.IsPopular
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            DeviceModels = new SelectList(await _db.DeviceModels.OrderBy(d => d.Name).ToListAsync(), "Id", "Name");

            if (!ModelState.IsValid) return Page();

            var price = await _db.ServiceItems.FirstOrDefaultAsync(p => p.Id == Input.Id);
            if (price == null) return NotFound();

            price.DeviceModelId = Input.DeviceModelId;
            price.ServiceName = Input.ServiceName;
            price.Description = Input.Description;
            price.Cost = Input.Cost;
            price.IsPopular = Input.IsPopular;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Услуга обновлена.";
            return RedirectToPage("./Index", new { FilterDeviceId = Input.DeviceModelId });
        }
    }
}
