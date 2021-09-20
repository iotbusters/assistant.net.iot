using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Assistant.Net.Scheduler.Api
{
    /// <summary/>
    public class Program
    {
        /// <summary/>
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        /// <summary/>
        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(b => b.UseStartup<Startup>())
            .UseConsoleLifetime();
    }
}
