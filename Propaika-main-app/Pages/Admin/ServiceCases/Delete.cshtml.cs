using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Propaika_main_app.Data;
using Propaika_main_app.Models;

namespace Propaika_main_app.Pages.Admin.ServiceCases
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;

        public DeleteModel(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [BindProperty]
        public ServiceCase ServiceCase { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            ServiceCase = await _db.ServiceCases.FindAsync(id);
            if (ServiceCase == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            var caseToDelete = await _db.ServiceCases.FindAsync(id);
            if (caseToDelete == null) return NotFound();

            // Удаляем файлы с диска
            DeleteFileIfExists(caseToDelete.BeforeImage);
            DeleteFileIfExists(caseToDelete.AfterImage);

            _db.ServiceCases.Remove(caseToDelete);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Кейс удалён!";
            return RedirectToPage("./Index");
        }

        private void DeleteFileIfExists(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
