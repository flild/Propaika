using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Extra;
using System.ComponentModel.DataAnnotations;

namespace Propaika_main_app.Pages.Admin.Devices
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public EditModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Название обязательно")]
            public string Name { get; set; } = string.Empty;

            [Required]
            public DeviceType DeviceType { get; set; } = DeviceType.Phone;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var device = await _db.DeviceModels.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (device == null) return NotFound();

            Input = new InputModel
            {
                Id = device.Id,
                Name = device.Name,
                DeviceType = device.DeviceType
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var device = await _db.DeviceModels.FirstOrDefaultAsync(d => d.Id == Input.Id);
            if (device == null) return NotFound();

            device.Name = Input.Name;
            device.DeviceType = Input.DeviceType;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Модель обновлена.";

            return RedirectToPage("./Index");
        }
    }
}
