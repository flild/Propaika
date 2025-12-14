using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
    public RepairRequest Form { get; set; } = new RepairRequest();

    public bool Submitted { get; set; }

    // Для секции "примеры ремонтов"
    public List<ServiceCase> CasesTeaser { get; private set; } = new();

    public async Task OnGetAsync()
    {
        CasesTeaser = await GetCasesTeaserAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // 1. Валидация
        if (!ModelState.IsValid)
        {
            // Собираем ошибки, чтобы показать их на клиенте (опционально)
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return new JsonResult(new { success = false, message = "Проверьте правильность заполнения полей", errors });
        }

        try
        {
            // 2. Сохранение в БД
            _db.RepairRequests.Add(Form);
            await _db.SaveChangesAsync();

            // 3. Отправка в ТГ (заглушка)
            await SendToTelegramAsync(Form);

            // 4. Возвращаем JSON с успехом
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Ошибка сервера. Попробуйте позже." });
        }
    }
    private async Task SendToTelegramAsync(RepairRequest request)
    {
        // TODO: Реализовать отправку уведомления в Telegram канал или боту
        // Пример:
        // var message = $"Новая заявка #{request.Id}\n{request.ClientName}\n{request.PhoneNumber}";
        // await _telegramBot.SendTextMessageAsync(chatId, message);

        await Task.CompletedTask; // Имитация асинхронной работы
    }
    private Task<List<ServiceCase>> GetCasesTeaserAsync()
    {
        return _db.ServiceCases
            .AsNoTracking()
            .Where(x => x.Slug != null && x.Slug != "")
            .Where(x => (x.BeforeImage != null && x.BeforeImage != "") ||
                        (x.AfterImage != null && x.AfterImage != ""))
            .OrderByDescending(x => x.DateCompleted)
            .Take(3) // можно 3/4/6
            .ToListAsync();
    }
}
