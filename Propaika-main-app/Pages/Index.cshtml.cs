using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Propaika.Web.Data;
using Propaika.Web.Data.Entities;
using Propaika_main_app.Data;
using Propaika_main_app.Models;

namespace Propaika.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public RepairRequest Form { get; set; } = new();

    public bool Submitted { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _db.RepairRequests.Add(Form);
        await _db.SaveChangesAsync();

        // TODO: здесь же будем слать сообщение в Telegram

        Submitted = true;
        ModelState.Clear();
        Form = new RepairRequest();

        return Page();
    }
}
