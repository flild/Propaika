using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Propaika_main_app.Services
{
    public class TelegramBotWorker : BackgroundService
    {

        private readonly TelegramQueue _queue;
        private readonly ILogger<TelegramBotWorker> _logger;
        private readonly TelegramBotClient _botClient;
        private readonly string _chatId;

        public TelegramBotWorker(TelegramQueue queue, IConfiguration config, ILogger<TelegramBotWorker> logger)
        {
            _queue = queue;
            _logger = logger;

            // Читаем настройки
            var token = config["TelegramSettings:BotToken"];
            _chatId = config["TelegramSettings:ChatId"];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(_chatId))
            {
                _logger.LogError("Telegram settings are missing in appsettings.json");
            }
            else
            {
                _botClient = new TelegramBotClient(token);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Telegram Worker started.");

            // Бесконечный цикл, пока приложение работает
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Ждем появления заявки в очереди (здесь поток "спит", ресурсы не ест)
                    var request = await _queue.DequeueAsync(stoppingToken);
                    string cleanPhone = new string(request.PhoneNumber.Where(char.IsDigit).ToArray());
                    if (cleanPhone.StartsWith("8")) cleanPhone = "7" + cleanPhone.Substring(1);
                    // 2. Формируем сообщение
                    var messageText =
                        $"🔥 <b>НОВАЯ ЗАЯВКА #{request.Id}</b>\n" +
                        $"➖➖➖➖➖➖➖➖➖➖➖\n" +
                        $"👤 <b>Клиент:</b> {request.ClientName}\n" +
                        $"📱 <b>Устройство:</b> {request.DeviceModel ?? "<i>Не указано</i>"}\n" +
                        $"🛠 <b>Проблема:</b>\n" +
                        $"<code>{request.ProblemDescription ?? "Без описания"}</code>\n" +
                        $"➖➖➖➖➖➖➖➖➖➖➖\n" +
                        $"📞 <b>Телефон:</b> {request.PhoneNumber}\n" +
                        $"🕒 <i>{request.CreatedAt:dd.MM.yyyy HH:mm}</i>";


                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        // Ряд 1: WhatsApp и Telegram
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("💬 WhatsApp", $"https://wa.me/{cleanPhone}"),
                            InlineKeyboardButton.WithUrl("✈️ Telegram", $"tg://resolve?phone={cleanPhone}")
                        }
                    });


                    // 3. Отправляем (если бот инициализирован)
                    if (_botClient != null)
                    {
                        await _botClient.SendMessage(
                            chatId: _chatId,
                            text: messageText,
                            parseMode: ParseMode.Html,
                            replyMarkup: inlineKeyboard, 
                            cancellationToken: stoppingToken
                        );

                        _logger.LogInformation($"Sent notification for Request #{request.Id}");
                    }
                }
                catch (OperationCanceledException)
                {
                    // Нормальная остановка
                    break;
                }
                catch (Exception ex)
                {
                    // Если Telegram API упал, мы не хотим ронять весь сервис, просто логируем
                    _logger.LogError(ex, "Error sending telegram notification");
                }
            }
        }
    }
}
