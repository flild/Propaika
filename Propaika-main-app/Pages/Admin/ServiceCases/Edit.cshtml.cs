using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Propaika_main_app.Data;
using Propaika_main_app.Models;
using Slugify;
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
        public CaseInputModel Input { get; set; } = new();

        // Класс InputModel перенесли сюда
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

            [Display(Name = "Фото ДО")]
            public IFormFile? UploadBefore { get; set; }

            [Display(Name = "Фото ПОСЛЕ")]
            public IFormFile? UploadAfter { get; set; }

            // Для отображения текущих картинок
            public string? CurrentBeforeImage { get; set; }
            public string? CurrentAfterImage { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                // РЕДАКТИРОВАНИЕ: Загружаем данные из БД
                var serviceCase = await _db.ServiceCases.FindAsync(id.Value);
                if (serviceCase == null) return NotFound();

                Input = new CaseInputModel
                {
                    Id = serviceCase.Id,
                    Title = serviceCase.Title,
                    Description = serviceCase.Description,
                    DeviceModel = serviceCase.DeviceModel,
                    ServiceType = serviceCase.ServiceType,
                    Cost = serviceCase.Cost,
                    CurrentBeforeImage = serviceCase.BeforeImage,
                    CurrentAfterImage = serviceCase.AfterImage
                };
            }
            // Если id нет, оставляем Input пустым для СОЗДАНИЯ
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            ServiceCase serviceCase;

            if (Input.Id.HasValue && Input.Id.Value > 0)
            {
                // UPDATE
                serviceCase = await _db.ServiceCases.FindAsync(Input.Id.Value);
                if (serviceCase == null) return NotFound();
            }
            else
            {
                // CREATE
                serviceCase = new ServiceCase();
                _db.ServiceCases.Add(serviceCase);
                serviceCase.DateCompleted = DateTime.UtcNow;
            }

            // Обновляем поля
            serviceCase.Title = Input.Title;
            serviceCase.Description = Input.Description;
            serviceCase.DeviceModel = Input.DeviceModel;
            serviceCase.ServiceType = Input.ServiceType;
            serviceCase.Cost = Input.Cost;

            // Генерация SEO (ваша логика)
            string baseMeta = $"{Input.ServiceType ?? "Ремонт"} {Input.DeviceModel ?? ""} {Input.Title}";
            serviceCase.MetaTitle = baseMeta.Trim();

            if (!string.IsNullOrEmpty(Input.Description))
            {
                var desc = Input.Description.Length > 150 ? Input.Description.Substring(0, 147) + "..." : Input.Description;
                serviceCase.MetaDescription = desc;
            }

            if (string.IsNullOrEmpty(serviceCase.Slug))
            {
                var config = new SlugHelperConfiguration();
                // Тут добавьте ваш словарь транслита, если он есть в классе, или вынесите его в отдельный сервис
                config.StringReplacements.Add("а", "a");
                // ... (ваша карта транслита) ...

                var helper = new SlugHelper(config);
                serviceCase.Slug = helper.GenerateSlug($"{Input.DeviceModel} {Input.Title}-{Guid.NewGuid().ToString().Substring(0, 4)}");
            }

            // Файлы
            if (Input.UploadBefore != null)
            {
                DeleteFile(serviceCase.BeforeImage);
                serviceCase.BeforeImage = await SaveFileAsync(Input.UploadBefore);
            }
            if (Input.UploadAfter != null)
            {
                DeleteFile(serviceCase.AfterImage);
                serviceCase.AfterImage = await SaveFileAsync(Input.UploadAfter);
            }

            await _db.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "cases");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var ext = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create)) await file.CopyToAsync(fileStream);
            return "/uploads/cases/" + uniqueFileName;
        }

        private void DeleteFile(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            var absolutePath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
            if (System.IO.File.Exists(absolutePath)) System.IO.File.Delete(absolutePath);
        }
    }
}
