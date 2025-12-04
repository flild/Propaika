using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Models;

namespace Propaika_main_app.Pages.Admin.Devices
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public DeleteModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public DeviceModel Device { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Device = await _db.DeviceModels
                .Include(d => d.ServiceItems)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (Device == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            var device = await _db.DeviceModels
                .Include(d => d.ServiceItems)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null) return NotFound();

            if (device.ServiceItems.Any())
            {
                ModelState.AddModelError(string.Empty, "Нельзя удалить модель, у которой есть услуги в прайсе.");
                Device = device;
                return Page();
            }

            _db.DeviceModels.Remove(device);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Модель удалена.";
            return RedirectToPage("./Index");
        }
    }
}
