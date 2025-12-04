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

        public IList<ServiceItem> Prices { get; set; } = new List<ServiceItem>();
        public SelectList DeviceModels { get; set; } = null!;

        // Для фильтрации
        [BindProperty(SupportsGet = true)]
        public int? FilterDeviceId { get; set; }

        // Для массового изменения
        [BindProperty]
        public BulkEditModel BulkEdit { get; set; } = new();

        public class BulkEditModel
        {
            [Display(Name = "Тип изменения")]
            public string ChangeType { get; set; } = "percent"; // percent или fixed

            [Display(Name = "Значение")]
            public decimal Value { get; set; }

            [Display(Name = "Применить к модели")]
            public int? TargetDeviceId { get; set; }
        }

        public async Task OnGetAsync()
        {
            var query = _db.ServiceItems
                .Include(p => p.DeviceModel)
                .AsQueryable();

            if (FilterDeviceId.HasValue)
            {
                query = query.Where(p => p.DeviceModelId == FilterDeviceId.Value);
            }

            Prices = await query.OrderBy(p => p.DeviceModel!.Name).ThenBy(p => p.ServiceName).ToListAsync();

            DeviceModels = new SelectList(await _db.DeviceModels.OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
        }

        public async Task<IActionResult> OnPostBulkEditAsync()
        {
            if (BulkEdit.Value == 0)
            {
                TempData["Error"] = "Укажите значение для изменения";
                return RedirectToPage();
            }

            var query = _db.ServiceItems.AsQueryable();

            // Если указана конкретная модель - применяем только к ней
            if (BulkEdit.TargetDeviceId.HasValue)
            {
                query = query.Where(p => p.DeviceModelId == BulkEdit.TargetDeviceId.Value);
            }

            var items = await query.ToListAsync();

            foreach (var item in items)
            {
                if (BulkEdit.ChangeType == "percent")
                {
                    // Изменение на процент: +10% или -10%
                    item.Cost = item.Cost + (item.Cost * BulkEdit.Value / 100);
                }
                else
                {
                    // Изменение на фиксированную сумму
                    item.Cost = item.Cost + BulkEdit.Value;
                }

                // Не даём уйти в минус
                if (item.Cost < 0) item.Cost = 0;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Обновлено {items.Count} позиций";
            return RedirectToPage();
        }
    }
}
