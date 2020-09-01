using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace dapper_pubsub_test.ServiceWorkers
{
    public class ModProcessorWorker : BackgroundService
    {
        private readonly ILogger<ModProcessorWorker> _logger;
        private readonly ConcurrentQueue<CloudEvent> _messageQueue;

        public ModProcessorWorker(ILogger<ModProcessorWorker> logger, ConcurrentQueue<CloudEvent> messageQueue)
        {
            this._logger = logger;
            this._messageQueue = messageQueue;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Mod Processor Background Service is starting...");

            stoppingToken.Register(() =>
                _logger.LogInformation($"Mod Processor Background Service is stopping..."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Mod Processor Background Service doing background work...");

                /*
                 * Processed all queued messages
                 */
                if (_messageQueue != null && _messageQueue.Count > 0)
                {
                    while (_messageQueue.TryDequeue(out CloudEvent message))
                    {
                        var strMsg = JsonSerializer.Serialize(message);
                        _logger.LogInformation($"Successfully Retrieved message with id [{message.Id}] and payload [{strMsg}] from concurrent queue.");
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }

            _logger.LogDebug($"Mod Processor Background Service is stopping.");
        }
    }
}