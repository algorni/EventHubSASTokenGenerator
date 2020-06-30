using Azure.Messaging.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace EventHubRESTClientApp
{
    /// <summary>
    /// Adapter Class from a list of Event Data to their JSON Reppresenatation for sending via the 
    /// Event Hub HTTP REST API
    /// </summary>
    public static class EHEventData2JSONAdapter
    {
        public static string ToRESTInterfaceJsonString(this IEnumerable<EventData> events)
        {
            // [{"Body":"Message1", "UserProperties":{"Alert":"Strong Wind"},"BrokerProperties":{"CorrelationId","32119834-65f3-48c1-b366-619df2e4c400"}},{"Body":"Message2"},{"Body":"Message3"}]  

            if (events != null)
            { 
                //this is not including the BrokerProperties as they are not carried by the EventData object (as far as understood)

                var wrapperPayload = from e in events
                                     select new { 
                                         Body = extractStringBody(e) 
                                         //UserProperties = JsonSerializer.Serialize(e.Properties)
                                     };

                var payload = JsonSerializer.Serialize(wrapperPayload.ToArray());

                return payload;          
            }

            return string.Empty;
        }


        private static object extractStringBody(EventData e)
        {
            //simplification here --> assume EventData body as a proper string
            var str = System.Text.Encoding.Default.GetString(e.Body.ToArray());

            return str;
        }
    }
}
