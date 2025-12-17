using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Propaika_main_app.Data;
using Propaika_main_app.Extra;
using Propaika_main_app.Models;

namespace Propaika_main_app.Pages.Admin.Requests
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) { _context = context; }

        public List<RepairRequest> Requests { get; set; } = new();

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalPages { get; set; }

        // Статистика для дашборда
        public int NewCount { get; set; }
        public int InProgressCount { get; set; }

        public void OnGet(int p = 1)
        {
            CurrentPage = p < 1 ? 1 : p;

            var query = _context.RepairRequests.AsQueryable();

            // Сортировка: Сначала Новые (0), потом В работе (1), потом Готовые (2).
            // Внутри групп - по дате (свежие сверху)
            query = query.OrderBy(r => r.Status)
                         .ThenByDescending(r => r.CreatedAt);

            // Статистика
            NewCount = query.Count(r => r.Status == RequestStatus.New);
            InProgressCount = query.Count(r => r.Status == RequestStatus.InProgress);

            var totalItems = query.Count();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            if (TotalPages < 1) TotalPages = 1;

            Requests = query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public IActionResult OnPostDelete(int id)
        {
            var item = _context.RepairRequests.Find(id);
            if (item != null)
            {
                _context.RepairRequests.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToPage(new { p = CurrentPage });
        }

        // Универсальный метод смены статуса
        public IActionResult OnPostSetStatus(int id, RequestStatus status)
        {
            var item = _context.RepairRequests.Find(id);
            if (item != null)
            {
                item.Status = status;
                _context.SaveChanges();
            }
            return RedirectToPage(new { p = CurrentPage });
        }
    }
}
