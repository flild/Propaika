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
            // Жадная загрузка с цепочкой Include -> ThenInclude
            var devices = await _db.DeviceModels
                .Include(d => d.DeviceModelServices.Where(dms => dms.IsEnabled)) // Фильтруем отключенные услуги сразу в БД
                    .ThenInclude(dms => dms.ServiceItem) // Подтягиваем данные самой услуги (название, описание)
                .OrderBy(d => d.Name) // Сортировка по алфавиту для удобства
                .ToListAsync();

            // Группируем по типу устройства (Phone, Tablet, Laptop...)
            DevicesByType = devices.ToLookup(d => d.DeviceType);
        }
    }
}