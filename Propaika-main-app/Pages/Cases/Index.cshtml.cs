using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Models;

namespace Propaika_main_app.Pages.Cases
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public IList<ServiceCase> Cases { get; set; } = new List<ServiceCase>();

        public async Task OnGetAsync()
        {
            Cases = await _db.ServiceCases
                .OrderByDescending(c => c.DateCompleted)
                .ToListAsync();
        }
    }
}
