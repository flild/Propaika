using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Propaika_main_app.Data;
using System.ComponentModel.DataAnnotations;

namespace Propaika_main_app.Pages.Admin.ServiceCases
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;

        public EditModel(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Укажите название")]
            public string Title { get; set; } = string.Empty;

            public string? Description { get; set; }

            [Required]
            [Range(0, 1000000)]
            public decimal Cost { get; set; }

            // Необязательные - если не загружаем новые, оставляем старые
            public IFormFile? UploadBefore { get; set; }
            public IFormFile? UploadAfter { get; set; }

            // Старые пути (для отображения)
            public string? ExistingBeforeImage { get; set; }
            public string? ExistingAfterImage { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var serviceCase = await _db.ServiceCases.FindAsync(id);
            if (serviceCase == null) return NotFound();

            Input = new InputModel
            {
                Id = serviceCase.Id,
                Title = serviceCase.Title,
                Description = serviceCase.Description,
                Cost = serviceCase.Cost,
                ExistingBeforeImage = serviceCase.BeforeImage,
                ExistingAfterImage = serviceCase.AfterImage
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var serviceCase = await _db.ServiceCases.FindAsync(Input.Id);
            if (serviceCase == null) return NotFound();

            // Обновляем текстовые поля
            serviceCase.Title = Input.Title;
            serviceCase.Description = Input.Description;
            serviceCase.Cost = Input.Cost;

            // Если загрузили новое фото ДО - удаляем старое и сохраняем новое
            if (Input.UploadBefore != null)
            {
                DeleteFileIfExists(serviceCase.BeforeImage);
                serviceCase.BeforeImage = await SaveFileAsync(Input.UploadBefore);
            }

            // Аналогично для фото ПОСЛЕ
            if (Input.UploadAfter != null)
            {
                DeleteFileIfExists(serviceCase.AfterImage);
                serviceCase.AfterImage = await SaveFileAsync(Input.UploadAfter);
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Кейс обновлён!";
            return RedirectToPage("./Index");
        }

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "cases");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return "/uploads/cases/" + uniqueName;
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
