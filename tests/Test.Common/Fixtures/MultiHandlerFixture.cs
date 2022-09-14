using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Test.Common.Fixtures;

public sealed class MultiHandlerFixture : IDisposable
{
    private static int callNumber;

    private readonly RemoteHandlerFixture[] fixtures;

    public MultiHandlerFixture(params RemoteHandlerFixture[] fixtures)
    {
        if (fixtures.Length == 0)
            throw new ArgumentException("At least one fixture is required.", nameof(fixtures));
        this.fixtures = fixtures;
    }

    public IMessagingClient Client => fixtures[callNumber++ % fixtures.Length].Client;

    public void Dispose()
    {
        foreach (var fixture in fixtures)
            fixture.Dispose();
    }
}
