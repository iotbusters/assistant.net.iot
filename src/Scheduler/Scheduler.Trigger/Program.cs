using Assistant.Net.Scheduler.Trigger.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace Assistant.Net.Scheduler.Trigger
{
    /// <summary/>
    public class Program
    {
        /// <summary/>
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        /// <summary/>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(b => b.AddTrigger(
                    b.Build().GetConnectionString(ConfigurationNames.Messaging),
                    Startup.MessagingDatabaseName))
                .ConfigureServices((ctx, services) => new Startup(ctx.Configuration).ConfigureServices(services));

            if (!OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
                return builder.UseConsoleLifetime();
            return builder;
        }
    }
}
