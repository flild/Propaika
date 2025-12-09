using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Models;
using System.ComponentModel.DataAnnotations;

namespace Propaika_main_app.Pages.Admin.Prices
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public IList<ServiceViewModel> ServiceList { get; set; } = new List<ServiceViewModel>();
        public SelectList DeviceModels { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public int? FilterDeviceId { get; set; }

        [BindProperty]
        public ServiceInputModel Input { get; set; } = new();

        [BindProperty]
        public BulkEditModel BulkEdit { get; set; } = new();

        public class ServiceViewModel
        {
            public int Id { get; set; } // ID самой услуги
            public string ServiceName { get; set; }
            public string Description { get; set; }
            public bool IsPopular { get; set; }

            // Данные специфичные для контекста (фильтра)
            public decimal DisplayCost { get; set; } // Цена для показа (либо конкретная, либо min-max)
            public bool IsEnabled { get; set; }
            public bool IsVaryingPrice { get; set; } // Флаг: цены разные на разных моделях
        }

        public class ServiceInputModel
        {
            public int? Id { get; set; }
            [Required] public string ServiceName { get; set; }
            public string? Description { get; set; }
            public bool IsPopular { get; set; }

            [Range(0, 1000000)]
            public decimal BaseCost { get; set; } // Цена для сохранения
        }

        public class BulkEditModel
        {
            public string ChangeType { get; set; } = "percent"; // percent, fixed, set_exact
            public int? TargetDeviceId { get; set; }
            public decimal Value { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            DeviceModels = new SelectList(await _db.DeviceModels.OrderBy(d => d.Name).ToListAsync(), "Id", "Name");

            var query = _db.ServiceItems
                .Include(s => s.DeviceModelServices)
                .AsQueryable();

            var services = await query.ToListAsync();

            ServiceList = services.Select(s => {
                var vm = new ServiceViewModel
                {
                    Id = s.Id,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    IsPopular = s.IsPopular,
                };

                if (FilterDeviceId.HasValue)
                {
                    // Режим одной модели
                    var link = s.DeviceModelServices.FirstOrDefault(x => x.DeviceModelId == FilterDeviceId.Value);
                    vm.DisplayCost = link?.Cost ?? 0;
                    vm.IsEnabled = link?.IsEnabled ?? false;
                    vm.IsVaryingPrice = false;
                }
                else
                {
                    // Общий режим
                    var costs = s.DeviceModelServices.Select(x => x.Cost).ToList();
                    if (costs.Any())
                    {
                        vm.DisplayCost = costs.Max(); // Показываем самую высокую цену как ориентир
                        vm.IsVaryingPrice = costs.Distinct().Count() > 1;
                    }
                    vm.IsEnabled = true;
                }
                return vm;
            })
            .OrderBy(s => s.ServiceName)
            .ToList();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            if (Input.Id.HasValue) // EDIT
            {
                var service = await _db.ServiceItems
                    .Include(s => s.DeviceModelServices)
                    .FirstOrDefaultAsync(s => s.Id == Input.Id);

                if (service != null)
                {
                    service.ServiceName = Input.ServiceName;
                    service.Description = Input.Description;
                    service.IsPopular = Input.IsPopular;

                    if (FilterDeviceId.HasValue)
                    {
                        // Редактируем цену ТОЛЬКО для одной модели
                        var link = service.DeviceModelServices.FirstOrDefault(x => x.DeviceModelId == FilterDeviceId.Value);
                        if (link != null)
                        {
                            link.Cost = Input.BaseCost;
                        }
                        else
                        {
                            // Если связи не было, создаем
                            service.DeviceModelServices.Add(new DeviceModelServiceItem
                            {
                                DeviceModelId = FilterDeviceId.Value,
                                Cost = Input.BaseCost,
                                IsEnabled = true
                            });
                        }
                    }
                    else
                    {
                        // Глобально: обновляем цену ВСЕМ моделям
                        foreach (var link in service.DeviceModelServices)
                        {
                            link.Cost = Input.BaseCost;
                        }

                        // Если вдруг появились новые модели без связей, можно их тут создать (опционально)
                    }
                }
            }
            else // CREATE
            {
                var newService = new ServiceItem
                {
                    ServiceName = Input.ServiceName,
                    Description = Input.Description,
                    IsPopular = Input.IsPopular
                };

                _db.ServiceItems.Add(newService);
                await _db.SaveChangesAsync(); // Получаем ID

                // Создаем записи для ВСЕХ моделей
                var allModels = await _db.DeviceModels.Select(x => x.Id).ToListAsync();
                foreach (var mId in allModels)
                {
                    _db.DeviceModelServices.Add(new DeviceModelServiceItem
                    {
                        DeviceModelId = mId,
                        ServiceItemId = newService.Id,
                        Cost = Input.BaseCost, // Базовая цена для всех
                        IsEnabled = true
                    });
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToPage(new { FilterDeviceId });
        }

        // Массовое изменение (Bulk Edit)
        public async Task<IActionResult> OnPostBulkEditAsync()
        {
            if (BulkEdit.Value == 0 && BulkEdit.ChangeType != "set_exact")
            {
                // Защита от холостого прогона
                return RedirectToPage(new { FilterDeviceId });
            }

            var query = _db.DeviceModelServices.AsQueryable();

            // Если в форме массового изменения выбрали модель - фильтруем
            if (BulkEdit.TargetDeviceId.HasValue)
            {
                query = query.Where(x => x.DeviceModelId == BulkEdit.TargetDeviceId.Value);
            }
            // Иначе меняем ВСЕМ (глобально)

            var links = await query.ToListAsync();
            foreach (var link in links)
            {
                if (BulkEdit.ChangeType == "percent")
                    link.Cost += (link.Cost * BulkEdit.Value / 100);
                else if (BulkEdit.ChangeType == "fixed")
                    link.Cost += BulkEdit.Value;
                else if (BulkEdit.ChangeType == "set_exact")
                    link.Cost = BulkEdit.Value;

                if (link.Cost < 0) link.Cost = 0;
            }

            await _db.SaveChangesAsync();
            return RedirectToPage(new { FilterDeviceId });
        }




        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var item = await _db.ServiceItems.FindAsync(id);
            if (item != null)
            {
                _db.ServiceItems.Remove(item);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage(new { FilterDeviceId });
        }

        // TOGGLE (AJAX) - Вкл/Выкл услугу для модели
        public async Task<IActionResult> OnPostToggleDeviceAsync(int serviceId, int deviceId, bool state)
        {
            var link = await _db.DeviceModelServices
                .FirstOrDefaultAsync(x => x.ServiceItemId == serviceId && x.DeviceModelId == deviceId);

            if (link == null)
            {
                // Попытка найти цену у других моделей этой же услуги, чтобы не ставить 0
                var existingCost = await _db.DeviceModelServices
                    .Where(x => x.ServiceItemId == serviceId)
                    .Select(x => x.Cost)
                    .FirstOrDefaultAsync();

                // Если совсем ничего нет, будет 0
                if (existingCost == 0) existingCost = 0;

                link = new DeviceModelServiceItem
                {
                    ServiceItemId = serviceId,
                    DeviceModelId = deviceId,
                    IsEnabled = state,
                    Cost = existingCost
                };
                _db.DeviceModelServices.Add(link);
            }
            else
            {
                link.IsEnabled = state;
            }

            await _db.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }
    }
}
