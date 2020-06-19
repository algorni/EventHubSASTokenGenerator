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
    public class EHDataProducer : IHostedService, IDisposable
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly IOptions<EHDataProducerConfig> _config;

        private readonly Random _rnd = new Random();

        private readonly EHRestApiClient ehRestApiClient;

        private static int gen = 0;


        public EHDataProducer(ILogger<EHDataProducer> logger, IOptions<EHDataProducerConfig> config)
        {
            _logger = logger;
            _config = config;

            DateTime expireTime = DateTime.UtcNow.AddDays(1);

            ehRestApiClient = new EHRestApiClient(logger, _config.Value.EventHubConnectionString, expireTime);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Event Hub Data Producer with following configuration: " + JsonSerializer.Serialize( _config.Value) );

            while (!cancellationToken.IsCancellationRequested)
            {
                for (int par = 0; par < _config.Value.ParallelSender; par++) 
                {
                    Task.Factory.StartNew(() => sendEventHubData());
                }

                //ok now just want some time before the next shot of execution  
                //eventually if the MaxTimeBetweenOperationInMillieconds is short enought we could have additional parallel executions!
                var waitBeforeNext = _rnd.Next(0, _config.Value.MaxTimeBetweenSendBatchOperationInMillieconds);

                Task.Delay(waitBeforeNext).Wait();
            }

            return Task.CompletedTask;
        }

        DateTime lastMetric = DateTime.UtcNow;


        private async Task sendEventHubData()
        {
            var correlationID = Guid.NewGuid().ToString();

            _logger.LogInformation($"{correlationID},Begin Sending Data to Event Hub");


            MessagePayload message = new MessagePayload
            (){ CorrelationId = correlationID, Gen = gen++ , 
                Body = "alskdjfhslfjkasdhfqcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygqalskdjfhslfjkasdhfkjlhsdljkfasdhfsjkdlfhsduifysa90d87uy qcn509tq7349uh356423uyp862yh35uo6yh253uipt793q7rwgyasfdp7igyaghvscygq7hlt23pig8yavsfgyp9q54eghioq3e576h247otygq" };


            var response = await ehRestApiClient.SendSingleMessageAsync(message);


            _logger.LogInformation($"{correlationID},Completed Sending Data to Event Hub,{response.StatusCode.ToString()}");


            if ((double)gen % 1000.0 == 0)
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


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Event Hub Sender.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogInformation("LogProducer....");

        }
    }
}
