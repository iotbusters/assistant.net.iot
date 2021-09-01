using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Assistant.Net.Scheduler.Api
{
    /// <summary/>
    public class Program
    {
        /// <summary/>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(b => b
                .ConfigureAppConfiguration((ctx, cb) => cb
                    .AddEnvironmentVariables("ASSISTANTNET")
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
                .UseStartup<Startup>());
    }
}
