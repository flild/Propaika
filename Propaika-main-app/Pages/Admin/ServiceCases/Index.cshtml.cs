using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Slugify;
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
            [Display(Name = "Название работы")]
            public string Title { get; set; } = "";

            [Display(Name = "Описание проблемы и решения")]
            public string? Description { get; set; }

            [Display(Name = "Модель устройства")]
            public string? DeviceModel { get; set; }

            [Display(Name = "Тип услуги")]
            public string? ServiceType { get; set; }

            [Range(0, 1000000)]
            [Display(Name = "Стоимость (₽)")]
            public decimal Cost { get; set; }

            // Файлы
            [Display(Name = "Фото ДО")]
            public IFormFile? UploadBefore { get; set; }

            [Display(Name = "Фото ПОСЛЕ")]
            public IFormFile? UploadAfter { get; set; }

            // Скрытые поля для UI (чтобы показать текущие картинки при редактировании)
            public string? ExistingBeforePath { get; set; }
            public string? ExistingAfterPath { get; set; }

            // SEO поля убрали из Input, они генерируются автоматически
        }

        public async Task OnGetAsync(int? id)
        {

            Cases = await _db.ServiceCases
                .OrderByDescending(c => c.DateCompleted)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                // Если ошибка, нужно перезагрузить список, чтобы страница не сломалась
                await OnGetAsync(null);
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
                // Дата завершения ставится текущая при создании
                serviceCase.DateCompleted = DateTime.UtcNow;
            }

            // Основные поля
            serviceCase.Title = Input.Title;
            serviceCase.Description = Input.Description;
            serviceCase.DeviceModel = Input.DeviceModel;
            serviceCase.ServiceType = Input.ServiceType;
            serviceCase.Cost = Input.Cost;

            // АВТОМАТИЧЕСКОЕ SEO 
            // Формируем заголовок для SEO: "Ремонт iPhone 12 Pro - Замена стекла"
            string baseMeta = $"{Input.ServiceType ?? "Ремонт"} {Input.DeviceModel ?? ""} {Input.Title}";
            serviceCase.MetaTitle = baseMeta.Trim();

            // Meta Description берем из описания, обрезаем если длинное
            if (!string.IsNullOrEmpty(Input.Description))
            {
                var desc = Input.Description.Length > 150 ? Input.Description.Substring(0, 147) + "..." : Input.Description;
                serviceCase.MetaDescription = desc;
            }

            // Slugify генерация
            // Создаем конфиг для SlugHelper
            if (string.IsNullOrEmpty(serviceCase.Slug)) 
            {
                var config = new SlugHelperConfiguration();
                foreach (var item in GetRussianTranslitMap())
                {
                    if (!config.StringReplacements.ContainsKey(item.Key))
                    {
                        config.StringReplacements.Add(item.Key, item.Value);
                    }
                }

                var helper = new SlugHelper(config);
                string slugSource = $"{Input.DeviceModel} {Input.Title}";

                // Генерируем слаг и добавляем случайный хвост для уникальности (по желанию)
                serviceCase.Slug = helper.GenerateSlug(slugSource);
            }

            // Обработка файлов
            if (Input.UploadBefore != null)
            {
                if (!string.IsNullOrEmpty(serviceCase.BeforeImage)) DeleteFile(serviceCase.BeforeImage);
                serviceCase.BeforeImage = await SaveFileAsync(Input.UploadBefore);
            }

            if (Input.UploadAfter != null)
            {
                if (!string.IsNullOrEmpty(serviceCase.AfterImage)) DeleteFile(serviceCase.AfterImage);
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

            return RedirectToPage();
        }

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "cases");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // Сохраняем расширение файла, но имя делаем безопасным
            var ext = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
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

        // Словарь для транслитерации (Slugify.Core не умеет в кириллицу из коробки)
        private Dictionary<string, string> GetRussianTranslitMap()
        {
            return new Dictionary<string, string>
            {
                {"а", "a"}, {"б", "b"}, {"в", "v"}, {"г", "g"}, {"д", "d"}, {"е", "e"}, {"ё", "yo"},
                {"ж", "zh"}, {"з", "z"}, {"и", "i"}, {"й", "y"}, {"к", "k"}, {"л", "l"}, {"м", "m"},
                {"н", "n"}, {"о", "o"}, {"п", "p"}, {"р", "r"}, {"с", "s"}, {"т", "t"}, {"у", "u"},
                {"ф", "f"}, {"х", "h"}, {"ц", "ts"}, {"ч", "ch"}, {"ш", "sh"}, {"щ", "sch"}, {"ъ", ""},
                {"ы", "y"}, {"ь", ""}, {"э", "e"}, {"ю", "yu"}, {"я", "ya"}
            };
        }
    }
}

