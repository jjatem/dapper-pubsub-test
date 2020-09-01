using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using dapper_pubsub_test.Models;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace dapper_pubsub_test.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagesController : ControllerBase
    {
        private static readonly string pubsubName = "pubsub";
        private readonly ILogger<MessagesController> _logger;
        private readonly ConcurrentQueue<CloudEvent> _messageQueue;
        private readonly DaprClient _client;

        public MessagesController(ILogger<MessagesController> logger, ConcurrentQueue<CloudEvent> messageQueue)
        {
            _logger = logger;
            _messageQueue = messageQueue;

            /*
             * Initialize dapper client
             */
            var jsonOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };

            var client = new DaprClientBuilder()
                .UseJsonSerializationOptions(jsonOptions)
                .Build();

            this._client = client;
        }

        [HttpPost]
        public async Task<IActionResult> PostApproval(Message msg)
        {

            if (ModelState.IsValid)
            {
                await _client.PublishEventAsync(pubsubName, "mbsi_mod_approval", msg);
                var message = JsonSerializer.Serialize(msg);
                _logger.LogInformation($"Successfully published message: [{message}]");
            }

            return Ok(msg);
        }

        [HttpGet("subscribe")]
        public IActionResult Subscribe()
        {
            var payload = new[]
           {
                 new { topic = "mbsi_mod_approval", route = "/receive" }
            };
            return Ok(payload);
        }

        [Topic("pubsub", "mbsi_mod_approval")]
        [HttpPost]
        [Route("receive")]
        public IActionResult receive(CloudEvent msg)
        {
            var message = JsonSerializer.Serialize(msg);
            _logger.LogInformation($"Successfully RECEIVED message: [{message}]");
            _messageQueue.Enqueue(msg);
            _logger.LogInformation("Cloud Event message received and added to background worker processing queue.");
            return Ok();
        }

    }
}
