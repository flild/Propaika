using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Models;

namespace Propaika_main_app.Pages.Admin.Prices
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public DeleteModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public ServiceItem PriceItem { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            PriceItem = await _db.ServiceItems
                .Include(p => p.DeviceModel)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (PriceItem == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            var item = await _db.ServiceItems.FindAsync(id);
            if (item == null) return NotFound();

            _db.ServiceItems.Remove(item);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Услуга удалена.";
            return RedirectToPage("./Index");
        }
    }
}
