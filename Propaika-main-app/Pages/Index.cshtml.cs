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
    private readonly string _serverKey = "_"; 

    public IndexModel(ApplicationDbContext db, TelegramQueue telegramQueue, IConfiguration config)
    {
        _db = db;
        _telegramQueue = telegramQueue;
        _serverKey = config["Secrets:YandexCaptcha"];
    }

    [BindProperty]
    public RepairRequest Form { get; set; } = new RepairRequest();

    public bool Submitted { get; set; }

    // Для секции "примеры ремонтов"
    public List<ServiceCase> CasesTeaser { get; private set; } = new();

    public sealed class SmartCaptchaResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? Host { get; set; }
    }

    public async Task OnGetAsync()
    {
        CasesTeaser = await GetCasesTeaserAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var token = Request.Form["smart-token"].ToString();

        if (string.IsNullOrWhiteSpace(token))
            return new JsonResult(new { success = false, message = "Пожалуйста, подтвердите, что вы не робот." });

        try
        {


            // ip желательно передавать (за прокси настрой ForwardedHeaders)
            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

            var form = new Dictionary<string, string?>
            {
                ["secret"] = _serverKey,
                ["token"] = token,
                ["ip"] = userIp
            };

            using var content = new FormUrlEncodedContent(form.Where(kv => !string.IsNullOrEmpty(kv.Value))
                                                              .ToDictionary(kv => kv.Key, kv => kv.Value!));

            var resp = await client.PostAsync("https://smartcaptcha.cloud.yandex.ru/validate", content);
            var json = await resp.Content.ReadAsStringAsync();

            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = System.Text.Json.JsonSerializer.Deserialize<SmartCaptchaResponse>(json, options);

            if (!resp.IsSuccessStatusCode || result?.Status != "ok")
                return new JsonResult(new { success = false, message = "Проверка капчи не пройдена. Обновите страницу и попробуйте снова." });
        }
        catch
        {
            return new JsonResult(new { success = false, message = "Ошибка проверки капчи. Попробуйте позже." });
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
