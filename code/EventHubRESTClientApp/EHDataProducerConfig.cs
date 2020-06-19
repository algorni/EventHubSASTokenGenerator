using System;
using System.Collections.Generic;
using System.Text;

namespace EventHubRESTClientApp
{
    public class EHDataProducerConfig
    {
        public string EventHubConnectionString { get; set; }

        public int BatchSize { get; set; }

        public int ParallelSender { get; set; }


        public int MaxTimeBetweenSendBatchOperationInMillieconds { get; set; }
    }
}
