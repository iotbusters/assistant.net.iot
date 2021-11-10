using Microsoft.Extensions.Hosting;
using System;

namespace Assistant.Net.Scheduler.EventHandler
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
                .ConfigureServices((ctx, services) => new Startup(ctx.Configuration).ConfigureServices(services));

            if (!OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
                return builder.UseConsoleLifetime();
            return builder;
        }
    }
}
