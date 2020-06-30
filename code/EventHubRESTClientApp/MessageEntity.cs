using Azure.Messaging.EventHubs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EventHubRESTClientApp
{
    public class MessageEntity
    {
        public static int CurrentGen = 0;



        public int Gen { get; set; }

        public string Payload { get; set; }



        public static List<EventData> GetBatch(int batchSize)
        {
            List<EventData> batch = new List<EventData>();

            for (var i = 0; i < batchSize; i++)
            {
                MessageEntity message = new MessageEntity()
                {
                    Gen = CurrentGen++,
                    //just random stuff
                    Payload = generateRandomStuff()
                };

                var messageEntityJson = JsonSerializer.Serialize(message);

                var eventBodyBytes = Encoding.UTF8.GetBytes(messageEntityJson);

                var eventData = new EventData(eventBodyBytes);

                //this should result in almost 1.5KB of message

                batch.Add(eventData);           
            }

            return batch;
        }

        private static string generateRandomStuff()
        {
            List<string> justNoise = new List<string>();

            for (int i = 0; i < 26; i++)
            {
                justNoise.Add(Guid.NewGuid().ToString());
            }

            

            return string.Join("-", justNoise);
        }
    }
}
