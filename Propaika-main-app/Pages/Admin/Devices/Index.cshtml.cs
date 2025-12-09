using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Extra;
using Propaika_main_app.Models;
using System.ComponentModel.DataAnnotations;

namespace Propaika_main_app.Pages.Admin.Devices
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public IList<DeviceModel> Devices { get; set; } = new List<DeviceModel>();

        // Для Dropdown с типами устройств (enum)
        public SelectList DeviceTypes { get; set; } = null!;

        [BindProperty]
        public DeviceInputModel Input { get; set; } = new();

        public class DeviceInputModel
        {
            public int? Id { get; set; }

            [Required(ErrorMessage = "Название обязательно")]
            public string Name { get; set; } = "";

            [Required]
            public DeviceType DeviceType { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            // Загружаем список устройств
            Devices = await _db.DeviceModels
                .OrderBy(d => d.DeviceType)
                .ThenBy(d => d.Name)
                .ToListAsync();

            // Заполняем SelectList из Enum DeviceType
            DeviceTypes = new SelectList(Enum.GetValues<DeviceType>());
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            if (Input.Id.HasValue && Input.Id.Value > 0) // EDIT
            {
                var device = await _db.DeviceModels.FindAsync(Input.Id.Value);
                if (device != null)
                {
                    device.Name = Input.Name;
                    device.DeviceType = Input.DeviceType;
                }
            }
            else // CREATE
            {
                var newDevice = new DeviceModel
                {
                    Name = Input.Name,
                    DeviceType = Input.DeviceType
                };

                _db.DeviceModels.Add(newDevice);
                await _db.SaveChangesAsync(); // Получаем ID

                // АВТОМАТИЗАЦИЯ: Копируем услуги для нового устройства
                // Берем все существующие услуги
                var allServices = await _db.ServiceItems.ToListAsync();

                if (allServices.Any())
                {
                    // Опционально: можно найти "похожее" устройство того же типа, чтобы скопировать цены
                    // Но для простоты пока берем 0 или базовую логику.
                    // Лучший вариант: просто создать связи, цену админ настроит потом или она подтянется

                    foreach (var s in allServices)
                    {
                        _db.DeviceModelServices.Add(new DeviceModelServiceItem
                        {
                            DeviceModelId = newDevice.Id,
                            ServiceItemId = s.Id,
                            IsEnabled = true,
                            Cost = 0 // Или s.BaseCost если бы мы его хранили в ServiceItem
                        });
                    }
                    await _db.SaveChangesAsync();
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var device = await _db.DeviceModels.FindAsync(id);
            if (device != null)
            {
                // Каскадное удаление связей обычно настроено в БД, 
                // но EF Core сам удалит зависимые DeviceModelServices
                _db.DeviceModels.Remove(device);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}