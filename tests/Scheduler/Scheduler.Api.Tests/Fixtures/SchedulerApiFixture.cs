using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures;

public sealed class SchedulerApiFixture : IDisposable
{
    private readonly ServiceProvider provider;
    private readonly IHost host;

    public SchedulerApiFixture(ServiceProvider provider, IHost host)
    {
        this.provider = provider;
        this.host = host;
    }

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

    public StringContent Content(object content)
    {
        var options = provider.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        var json = JsonSerializer.Serialize(content, content.GetType(), options);
        return new(json)
        {
            Headers =
            {
                ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json) {CharSet = "utf-8"}
            }
        };
    }

    public StreamContent CreatedContent() => new(new MemoryStream());

    public StreamContent NoContent() => new(new MemoryStream());

    public void Dispose()
    {
        provider.Dispose();
        host.Dispose();
    }
}
