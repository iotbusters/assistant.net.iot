using Assistant.Net.Abstractions;
using System;

namespace Assistant.Net.Test.Common.Mocks;

public sealed class TestConfigureOptionsSource<T> : ConfigureOptionsSourceBase<T> where T : class
{
    public Action<T> ConfigureAction { get; set; } = delegate { };

    public override void Configure(T options) =>
        ConfigureAction(options);
}
