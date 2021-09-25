using Microsoft.Extensions.Hosting;

namespace Assistant.Net.Scheduler.EventHandler
{
    /// <summary/>
    public class Program
    {
        /// <summary/>
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        /// <summary/>
        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) => new Startup(ctx.Configuration).ConfigureServices(services))
            .UseConsoleLifetime();
    }
}
