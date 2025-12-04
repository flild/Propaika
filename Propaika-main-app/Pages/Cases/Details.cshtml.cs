using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Models;

namespace Propaika_main_app.Pages.Cases
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public DetailsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public ServiceCase Case { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            Case = await _db.ServiceCases
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (Case == null)
                return NotFound();

            // SEO meta-теги
            ViewData["Title"] = Case.MetaTitle ?? $"{Case.Title} - Ремонт {Case.DeviceModel}";
            ViewData["Description"] = Case.MetaDescription ?? Case.Description?.Substring(0, Math.Min(160, Case.Description?.Length ?? 0));
            ViewData["OgImage"] = Case.AfterImage; // Фото "после" как превью

            return Page();
        }
    }
}
