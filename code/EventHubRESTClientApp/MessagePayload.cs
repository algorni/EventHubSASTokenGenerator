using System;
using System.Collections.Generic;
using System.Text;

namespace EventHubRESTClientApp
{
    public class MessagePayload
    {
        public string CorrelationId { get; set; }

        public int Gen { get; set; }

        public string Body { get; set; }
    }
}
