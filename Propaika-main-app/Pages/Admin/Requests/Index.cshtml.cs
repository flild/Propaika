using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Propaika_main_app.Data;
using Propaika_main_app.Models;

namespace Propaika_main_app.Pages.Admin.Requests
{
    public enum RequestStatus { New, InProgress, Done }
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) { _context = context; }

        public List<RepairRequest> Requests { get; set; } = new();

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 15; 
        public int TotalPages { get; set; }
        public int NewCount { get; set; }

        public void OnGet(int p = 1)
        {
            CurrentPage = p < 1 ? 1 : p;

            // --- ЗАМЕНИТЬ НА РЕАЛЬНУЮ БД ---
            var query = _context.RepairRequests.AsQueryable();
            // -------------------------------

            // Сначала новые, потом старые (по дате убывания)
            query = query.OrderBy(r => r.IsProcessed).ThenByDescending(r => r.CreatedAt);

            // Статистика
            NewCount = query.Count(r => !r.IsProcessed);

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

        public IActionResult OnPostToggleStatus(int id)
        {
             var item = _context.RepairRequests.Find(id);
             if (item != null) 
            { 
                 item.IsProcessed = !item.IsProcessed;
                 _context.SaveChanges(); 
            }

            return RedirectToPage(new { p = CurrentPage });
        }
    }
}
