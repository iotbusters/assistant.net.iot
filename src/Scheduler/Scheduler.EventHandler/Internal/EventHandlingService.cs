using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Internal
{
    internal class EventHandlingService : BackgroundService
    {
        public EventHandlingService()//IPartitionedStorage<Guid, > storage)
        {

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
