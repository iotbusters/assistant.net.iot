using Assistant.Net.Serialization.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures
{
    public class SchedulerApiFixture : IDisposable
    {
        private readonly ServiceProvider provider;

        public SchedulerApiFixture(ServiceProvider provider) =>
            this.provider = provider;

        public HttpClient Client => provider.GetRequiredService<HttpClient>();

        public Task<HttpResponseMessage> Get(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            return Client.SendAsync(request);
        }

        public Task<HttpResponseMessage> Post(string url, object? content = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (content != null) request.Content = Content(content);
            return Client.SendAsync(request);
        }

        public Task<HttpResponseMessage> Put(string url, object? content = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            if (content != null) request.Content = Content(content);
            return Client.SendAsync(request);
        }

        public Task<HttpResponseMessage> Delete(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            return Client.SendAsync(request);
        }

        public StreamContent Content(object content)
        {
            var stream = new MemoryStream();
            provider.GetRequiredService<IJsonSerializer>().Serialize(stream, content).Wait();
            stream.Position = 0;

            return new(stream)
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json) {CharSet = "utf-8"}
                }
            };
        }

        public StreamContent CreatedContent() => new(new MemoryStream())
        {
            Headers = { { HeaderNames.ContentLength, "0" } }
        };

        public StreamContent NoContent() => new(new MemoryStream());

        public virtual void Dispose() => provider.Dispose();
    }
}