using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Extra;
using Propaika_main_app.Models;

namespace Propaika_main_app.Pages.Prices
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        // Группировка: Тип устройства -> Список моделей (с ценами внутри)
        public ILookup<DeviceType, DeviceModel> DevicesByType { get; set; } = null!;

        public async Task OnGetAsync()
        {
            // Жадная загрузка (Include) цен сразу, сортировка моделей по новизне (Id desc или по имени)
            var devices = await _db.DeviceModels
                .Include(d => d.ServiceItems)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            // Группируем по типу (Phone, Tablet...)
            DevicesByType = devices.ToLookup(d => d.DeviceType);
        }
    }
}
