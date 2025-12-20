using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Extra;
using Propaika_main_app.Models;

namespace Propaika_main_app.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public int TotalCases { get; set; }
        public int TotalDevices { get; set; }
        public int TotalPriceItems { get; set; }
        public int PendingRequests { get; set; }

        public List<string> ChartLabels { get; set; } = new();
        public List<int> ChartData { get; set; } = new();

        // Последние заявки
        public List<RepairRequest> RecentRequests { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Базовая статистика
            TotalCases = await _db.ServiceCases.CountAsync();
            TotalDevices = await _db.DeviceModels.CountAsync();
            TotalPriceItems = await _db.ServiceItems.CountAsync();
            PendingRequests = await _db.RepairRequests.CountAsync(r => r.Status != RequestStatus.Done);

            // Последние 5 заявок (незавершенные)
            RecentRequests = await _db.RepairRequests
                .Where(r => r.Status != RequestStatus.Done)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            // --- Подготовка данных для графика (последние 30 дней) ---
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-29);

            // Получаем сгруппированные данные из БД
            var requestsByDate = await _db.RepairRequests
                .Where(r => r.CreatedAt >= startDate)
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Date, v => v.Count);

            // Заполняем пропуски нулями (чтобы график был непрерывным)
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                ChartLabels.Add(date.ToString("dd.MM")); // Формат для оси X
                ChartData.Add(requestsByDate.ContainsKey(date) ? requestsByDate[date] : 0);
            }
        }
    }
}
