using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Propaika_main_app.Data;
using Propaika_main_app.Models;
using Propaika_main_app.Services;

namespace Propaika.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly TelegramQueue _telegramQueue;

    public IndexModel(ApplicationDbContext db, TelegramQueue telegramQueue)
    {
        _db = db;
        _telegramQueue = telegramQueue;
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
        string captchaResponse = Request.Form["g-recaptcha-response"];

        if (string.IsNullOrEmpty(captchaResponse))
        {
            return new JsonResult(new { success = false, message = "Пожалуйста, подтвердите, что вы не робот (нажмите галочку)." });
        }
        try
        {
            using (var client = new HttpClient())
            {

                var secretKey = "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe";

                var verifyUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaResponse}";
               var response = await client.PostAsync(verifyUrl, null);
                var jsonString = await response.Content.ReadAsStringAsync();

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var captchaResult = System.Text.Json.JsonSerializer.Deserialize<RecaptchaResponse>(jsonString, options);

                if (captchaResult == null || !captchaResult.Success)
                {
                    return new JsonResult(new { success = false, message = "Проверка капчи не пройдена. Обновите страницу." });
                }
            }
        }
        catch (Exception)
        {
            // Если Google недоступен, можно либо пропустить, либо выдать ошибку.
            // Безопаснее выдать ошибку.
            return new JsonResult(new { success = true, message = "Ошибка проверки капчи. Попробуйте позже." });
        }

        // =========================================================


        // 2. Валидация модели
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return new JsonResult(new { success = false, message = "Проверьте правильность заполнения полей", errors });
        }

        try
        {
            // 3. Сохранение в БД
            _db.RepairRequests.Add(Form);
            await _db.SaveChangesAsync();

            // 4. Отправка в ТГ
            await SendToTelegramAsync(Form);

            // 5. Возвращаем JSON с успехом
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            // Логирование ex (лучше добавить _logger.LogError)
            return new JsonResult(new { success = false, message = "Ошибка сервера. Попробуйте позже." });
        }
    }
    private async Task SendToTelegramAsync(RepairRequest request)
    {
        await _telegramQueue.QueueBackgroundWorkItemAsync(request);
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
    public class RecaptchaResponse
    {
        public bool Success { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}
