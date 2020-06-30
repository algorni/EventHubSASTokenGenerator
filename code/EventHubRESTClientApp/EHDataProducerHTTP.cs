using Azure.Messaging.EventHubs;
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
    public class EHDataProducerHTTP : BackgroundService, IHostedService, IDisposable
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly IOptions<EHDataProducerConfig> _config;

        private readonly Random _rnd = new Random();

        private readonly EHRestApiClient ehRestApiClient;


        public EHDataProducerHTTP(ILogger<EHDataProducerHTTP> logger, IOptions<EHDataProducerConfig> config)
        {
            _logger = logger;
            _config = config;

            DateTime expireTime = DateTime.UtcNow.AddDays(1);

            ehRestApiClient = new EHRestApiClient(logger, _config.Value.EventHubConnectionString, expireTime);
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
                    var waitBeforeStarting= _rnd.Next(0, 100);
                    Task.Delay(waitBeforeStarting).Wait();

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

            //var response = await ehRestApiClient.SendSingleMessageAsync(message);

            var response = await ehRestApiClient.SendMessageBatchAsync(eventData);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug($"Completed Sending Data to Event Hub,{response.StatusCode.ToString()}");
            }
            else
            {
                _logger.LogInformation($"Completed Sending Data to Event Hub (with some issue),{response.StatusCode.ToString()}\n{response.Content.ReadAsStringAsync().Result}");

            }

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
