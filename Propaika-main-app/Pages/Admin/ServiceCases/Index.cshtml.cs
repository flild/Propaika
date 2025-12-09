using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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

        [BindProperty]
        public CaseInputModel Input { get; set; } = new();

        public class CaseInputModel
        {
            public int? Id { get; set; }

            [Required(ErrorMessage = "Заголовок обязателен")]
            public string Title { get; set; } = "";

            public string? Description { get; set; }

            [Display(Name = "Модель устройства")]
            public string? DeviceModel { get; set; }

            [Display(Name = "Тип услуги")]
            public string? ServiceType { get; set; }

            [Range(0, 1000000)]
            public decimal Cost { get; set; }

            public DateTime DateCompleted { get; set; } = DateTime.UtcNow;

            // Файлы
            public IFormFile? UploadBefore { get; set; }
            public IFormFile? UploadAfter { get; set; }

            // Скрытые поля для хранения старых путей при редактировании
            public string? ExistingBeforePath { get; set; }
            public string? ExistingAfterPath { get; set; }

            // SEO
            public string? Slug { get; set; }
            public string? MetaTitle { get; set; }
            public string? MetaDescription { get; set; }
        }

        public async Task OnGetAsync()
        {
            Cases = await _db.ServiceCases
                .OrderByDescending(c => c.DateCompleted)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            ServiceCase serviceCase;

            if (Input.Id.HasValue && Input.Id.Value > 0)
            {
                // EDIT
                serviceCase = await _db.ServiceCases.FindAsync(Input.Id.Value);
                if (serviceCase == null) return NotFound();
            }
            else
            {
                // CREATE
                serviceCase = new ServiceCase();
                _db.ServiceCases.Add(serviceCase);
            }

            // Заполняем поля
            serviceCase.Title = Input.Title;
            serviceCase.Description = Input.Description;
            serviceCase.DeviceModel = Input.DeviceModel;
            serviceCase.ServiceType = Input.ServiceType;
            serviceCase.Cost = Input.Cost;
            serviceCase.DateCompleted = DateTime.UtcNow;// Input.DateCompleted; 
            serviceCase.MetaTitle = Input.MetaTitle;
            serviceCase.MetaDescription = Input.MetaDescription;

            // Генерация Slug
            if (string.IsNullOrWhiteSpace(Input.Slug))
            {
                serviceCase.Slug = GenerateSlug(Input.Title);
            }
            else
            {
                serviceCase.Slug = Input.Slug;
            }

            // Обработка картинок
            // 1. До
            if (Input.UploadBefore != null)
            {
                // Если редактируем, удаляем старую
                if (Input.Id.HasValue) DeleteFile(serviceCase.BeforeImage);
                serviceCase.BeforeImage = await SaveFileAsync(Input.UploadBefore);
            }

            // 2. После
            if (Input.UploadAfter != null)
            {
                if (Input.Id.HasValue) DeleteFile(serviceCase.AfterImage);
                serviceCase.AfterImage = await SaveFileAsync(Input.UploadAfter);
            }

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var serviceCase = await _db.ServiceCases.FindAsync(id);
            if (serviceCase == null) return NotFound();

            DeleteFile(serviceCase.BeforeImage);
            DeleteFile(serviceCase.AfterImage);

            _db.ServiceCases.Remove(serviceCase);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Кейс удален";
            return RedirectToPage();
        }

        // --- Helpers ---

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "cases");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/cases/" + uniqueFileName;
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

        private string GenerateSlug(string title)
        {
            string str = title.ToLower();
            // Транслитерация (упрощенная) или просто замена кириллицы - лучше использовать библиотеку Slugify, 
            // но для примера простая замена пробелов:
            str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); // Удалить спецсимволы (для англ)
            str = Regex.Replace(str, @"\s+", "-").Trim();
            // Если названия на русском, тут нужен полноценный транслитератор.
            // Пока добавим рандом, чтобы избежать дублей
            return str + "-" + DateTime.Now.Ticks.ToString().Substring(10);
        }
    }
}

