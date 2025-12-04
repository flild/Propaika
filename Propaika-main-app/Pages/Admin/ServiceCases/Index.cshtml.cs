using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Models;

namespace Propaika_main_app.Pages.Admin.ServiceCases
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;

        public IndexModel(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        public IList<ServiceCase> Cases { get; set; } = new List<ServiceCase>();

        public async Task OnGetAsync()
        {
            Cases = await _db.ServiceCases
                .OrderByDescending(c => c.DateCompleted)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var serviceCase = await _db.ServiceCases.FindAsync(id);
            if (serviceCase == null) return NotFound();

            // Удаляем файлы картинок, чтобы не мусорить
            DeleteFile(serviceCase.BeforeImage);
            DeleteFile(serviceCase.AfterImage);

            _db.ServiceCases.Remove(serviceCase);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Кейс удален";
            return RedirectToPage();
        }

        private void DeleteFile(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            var absolutePath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }
    }
}

