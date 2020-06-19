using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EventHubRESTClientApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Event Hub!");

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appConfig.json",
                    optional: false,
                    reloadOnChange: true);

                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var serilogLogger = new LoggerConfiguration()
                         .WriteTo.RollingFile("Logs/log-{HalfHour}.log",
                          outputTemplate: "{Timestamp:yyyy-MM-ddTHH:mm:ss.fffK},{Level},{Message}{NewLine}")
                         .CreateLogger();

                    services.AddLogging(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Information);
                        builder.AddSerilog(logger: serilogLogger, dispose: true);
                    });

                    services.AddOptions();

                    services.Configure<EHDataProducerConfig>(hostContext.Configuration.GetSection("EHDataProducerConfig"));

                    services.AddSingleton<IHostedService, EHDataProducer>();
                })
                .ConfigureLogging((hostingContext, logging) => {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            await builder.RunConsoleAsync();
        }

    }
}
