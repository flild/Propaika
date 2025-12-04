using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Models;

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

        public async Task OnGetAsync()
        {
            Devices = await _db.DeviceModels
                .Include(d => d.ServiceItems)
                .OrderBy(d => d.DeviceType)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }
    }
}
