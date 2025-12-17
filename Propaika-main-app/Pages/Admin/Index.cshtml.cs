using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Extra;

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

        public async Task OnGetAsync()
        {
            TotalCases = await _db.ServiceCases.CountAsync();
            TotalDevices = await _db.DeviceModels.CountAsync();
            TotalPriceItems = await _db.ServiceItems.CountAsync();
            PendingRequests = await _db.RepairRequests.CountAsync(r => r.Status != RequestStatus.Done);
        }
    }
}
