using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SASTokenGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hi, let's generate your SAS Token for Event Hub!");

            if (args.Count() >= 3)
            {
                DateTime expireTime = DateTime.UtcNow;

                if (args.Count() == 4)
                {
                    string minutesOfValidityStr = args[3];
                    int minutesOfValidity = int.Parse(minutesOfValidityStr);

                    expireTime += TimeSpan.FromMinutes(minutesOfValidity);
                }
                else
                {
                    //default 15 min expiry 
                    expireTime += TimeSpan.FromMinutes(15);
                }

                string token = createToken(args[0],args[1],args[2],expireTime);

                Console.WriteLine($"Token generated:\n{token}");
            }
            else
            {
                Console.WriteLine("I'm sorry you need to provide 3 parameters:  resourceUri, they keyName and key");
            }
        }

        private static string createToken(string resourceUri, string keyName, string key, DateTime expireTime)
        {
            TimeSpan sinceEpoch = expireTime - new DateTime(1970, 1, 1);
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }
    }
}
