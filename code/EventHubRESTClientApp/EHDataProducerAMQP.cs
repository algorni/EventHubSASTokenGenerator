using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubRESTClientApp
{
    public class EHDataProducerAMQP : BackgroundService, IHostedService, IDisposable
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly IOptions<EHDataProducerConfig> _config;

        private readonly Random _rnd = new Random();

        private readonly EventHubProducerClient ehProducerClient;


        public EHDataProducerAMQP(ILogger<EHDataProducerAMQP> logger, IOptions<EHDataProducerConfig> config)
        {
            _logger = logger;
            _config = config;

            DateTime expireTime = DateTime.UtcNow.AddDays(1);

            ehProducerClient = new EventHubProducerClient(_config.Value.EventHubConnectionString);
        }

        //public override Task StartAsync(CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Starting Event Hub Data Producer with following configuration: " + JsonSerializer.Serialize( _config.Value) );


        //    return Task.CompletedTask;
        //}

        DateTime lastMetric = DateTime.UtcNow;


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Event Hub Data Producer with following configuration: " + JsonSerializer.Serialize(_config.Value));

            while (!stoppingToken.IsCancellationRequested)
            {
                for (int par = 0; par < _config.Value.ParallelSender; par++)
                {
                    Task.Factory.StartNew(() => sendEventHubData());
                }

                //ok now just want some time before the next shot of execution  
                //eventually if the MaxTimeBetweenOperationInMillieconds is short enought we could have additional parallel executions!
                //var waitBeforeNext = _rnd.Next(0, _config.Value.MaxTimeBetweenSendBatchOperationInMillieconds);

                //Task.Delay(waitBeforeNext).Wait();

                //just wait a fixed and predictable amount of time
                Task.Delay(_config.Value.MaxTimeBetweenSendBatchOperationInMillieconds).Wait();
            }

            _logger.LogInformation("Stopping Event Hub Sender.");

            return Task.CompletedTask;
        }

        private async Task sendEventHubData()
        {
            _logger.LogDebug($"Begin Sending Data to Event Hub");

            List<EventData> eventData = MessageEntity.GetBatch(_config.Value.BatchSize);

            // Create a batch of events 
            using EventDataBatch eventBatch = await ehProducerClient.CreateBatchAsync();

            foreach(var item in eventData)
            {
                eventBatch.TryAdd(item);
            }

            // Use the producer client to send the batch of events to the event hub
            await ehProducerClient.SendAsync(eventBatch);



            //this code has to be reviewed as it seems not calculating properly the kpi :)
            if ((double)MessageEntity.CurrentGen % 1000.0 == 0)
            {
                var now = DateTime.UtcNow;
                TimeSpan elapsedTime = now - lastMetric;
                lastMetric = now;
                
                var totals = elapsedTime.TotalSeconds;

                var messSec = 1000.0 / totals;

                //toreview this calculation

                _logger.LogInformation($"{Guid.Empty},Sending {messSec} message / seconds");
            }
        }


        //public override Task StopAsync(CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Stopping Event Hub Sender.");
        //    return Task.CompletedTask;
        //}
    }
}
