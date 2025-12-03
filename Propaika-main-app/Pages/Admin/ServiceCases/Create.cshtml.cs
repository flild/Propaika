using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Propaika_main_app.Data;
using Propaika_main_app.Models;
using System.ComponentModel.DataAnnotations;

namespace Propaika_main_app.Pages.Admin.ServiceCases
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Укажите название")]
            public string Title { get; set; } = string.Empty;

            public string? Description { get; set; }

            [Required]
            [Range(0, 1000000)]
            public decimal Cost { get; set; }

            [Required(ErrorMessage = "Загрузите фото ДО")]
            public IFormFile UploadBefore { get; set; } = null!;

            [Required(ErrorMessage = "Загрузите фото ПОСЛЕ")]
            public IFormFile UploadAfter { get; set; } = null!;
        }

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var serviceCase = new ServiceCase
            {
                Title = Input.Title,
                Description = Input.Description,
                Cost = Input.Cost,
                BeforeImage = await SaveFileAsync(Input.UploadBefore),
                AfterImage = await SaveFileAsync(Input.UploadAfter),
                DateCompleted = DateTime.UtcNow
            };

            _db.ServiceCases.Add(serviceCase);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Кейс успешно добавлен!";
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
    }
}
