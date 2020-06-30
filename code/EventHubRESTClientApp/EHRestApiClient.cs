
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventHubs;

namespace EventHubRESTClientApp
{
    public class EHRestApiClient
    {
        private readonly ILogger _logger;


        private HttpClient httpClient = new HttpClient();

        private string sasToken;

        private string sbHost;

        private string entityPath;

        public EHRestApiClient(ILogger logger, string connectionString, DateTime expireTime)
        {
            _logger = logger;

            GenerateToken(connectionString, expireTime);
        }


        public void  GenerateToken(string connectionString, DateTime expireTime)
        {
            var connStringSplit = connectionString.Split(';');

            List<KeyValuePair<string, string>> kvPairs = new List<KeyValuePair<string, string>>();

            foreach (var item in connStringSplit)
            {
                var indexOfEqual = item.IndexOf("=");
                var kn = item.Substring(0, indexOfEqual);
                var vl = item.Replace(kn, string.Empty).TrimStart('=');
                kvPairs.Add(new KeyValuePair<string, string>(kn, vl));
            }

            entityPath = kvPairs.Where(i => i.Key == "EntityPath").First().Value;

            sbHost = kvPairs.Where(i => i.Key == "Endpoint").First().Value.Replace("sb://",string.Empty).TrimEnd('/');


            var keyName = kvPairs.Where(i => i.Key == "SharedAccessKeyName").First().Value;
            var key = kvPairs.Where(i => i.Key == "SharedAccessKey").First().Value;

           

            sasToken = createToken("sb://"+ sbHost, keyName, key, expireTime);
        }



        private string createToken(string resourceUri, string keyName, string key, DateTime expireTime)
        {
            TimeSpan sinceEpoch = expireTime - new DateTime(1970, 1, 1);
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }


        public async Task<HttpResponseMessage> SendSingleMessageAsync(object sendWhat)
        {
            // POST https://<yournamespace>.servicebus.windows.net/<yourentity>/messages
            // Content-Type: application/json
            // Authorization: SharedAccessSignature sr=https%3A%2F%2F<yournamespace>.servicebus.windows.net%2F<yourentity>&sig=<yoursignature from code above>&se=1438205742&skn=KeyName
            // ContentType: application/atom+xml;type=entry;charset=utf-8


            var hrm = new HttpRequestMessage(HttpMethod.Post, "https://" + sbHost + $"/{entityPath}/messages?timeout=60&api-version=2014-01");
            hrm.Headers.Add("Authorization", sasToken);
            StringContent stc = new StringContent(JsonSerializer.Serialize(sendWhat), Encoding.UTF8, "application/json");

            hrm.Content = stc;

            var result =  await httpClient.SendAsync(hrm);

            return result;
        }


        public async Task<HttpResponseMessage> SendMessageBatchAsync(List<EventData> batch)
        {
            // POST https://your-namespace.servicebus.windows.net/your-event-hub/messages?timeout=60&api-version=2014-01 HTTP/1.1  
            // Authorization: SharedAccessSignature sr=your-namespace.servicebus.windows.net&sig=your-sas-key&se=1456197782&skn=RootManageSharedAccessKey  
            // Content-Type: application/vnd.microsoft.servicebus.json  
            // Host: your-namespace.servicebus.windows.net  
            // [{"Body":"Message1", "UserProperties":{"Alert":"Strong Wind"},"BrokerProperties":{"CorrelationId","32119834-65f3-48c1-b366-619df2e4c400"}},{"Body":"Message2"},{"Body":"Message3"}]  

            var url = "https://" + sbHost + $"/{entityPath}/messages?timeout=60&api-version=2014-01";

            //var wrapperPayload = from b in batch
            //                    select new { Body = JsonSerializer.Serialize(b) };

            //var payload = JsonSerializer.Serialize(wrapperPayload);

            var payload = batch.ToRESTInterfaceJsonString();

            StringContent content = new StringContent(payload,Encoding.UTF8);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.microsoft.servicebus.json");
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", sasToken);

            DateTime startCallTime = DateTime.UtcNow;

            var result = await httpClient.PostAsync(url, content);

            TimeSpan callDuration = DateTime.UtcNow - startCallTime;

            return result;
        }
    }
}

