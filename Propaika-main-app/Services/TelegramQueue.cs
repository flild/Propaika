using Propaika_main_app.Models;
using System.Threading.Channels;

namespace Propaika_main_app.Services
{

    public class TelegramQueue
    {
        private readonly Channel<RepairRequest> _channel;

        public TelegramQueue()
        {
            // BoundedChannelOptions: если очередь полная, Wait - ждем освобождения
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<RepairRequest>(options);
        }
        // Метод для "Продюсера" (IndexModel)
        public async ValueTask QueueBackgroundWorkItemAsync(RepairRequest workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            await _channel.Writer.WriteAsync(workItem);
        }

        // Метод для "Консьюмера" (BackgroundService)
        public async ValueTask<RepairRequest> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
