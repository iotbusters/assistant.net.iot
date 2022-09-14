using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Mocks;

public sealed class TestStorage<TKey, TValue> : IAdminStorage<TKey, TValue>, IEnumerable<KeyValuePair<TKey,TValue>>
    where TKey : struct
{
    private readonly ConcurrentDictionary<TKey, TValue> storage = new();

    public Task<TValue> AddOrGet(TKey key, Func<TKey, Task<TValue>> addFactory, CancellationToken token = default) =>
        Task.FromResult(storage.GetOrAdd(key, _ => addFactory(key).Result));

    public Task<TValue> AddOrUpdate(
        TKey key,
        Func<TKey, Task<TValue>> addFactory,
        Func<TKey, TValue, Task<TValue>> updateFactory,
        CancellationToken token) =>
        Task.FromResult(storage.AddOrUpdate(key, _ => addFactory(key).Result, (k, v) => updateFactory(k, v).Result));

    public Task<Option<TValue>> TryGet(TKey key, CancellationToken token = default)
    {
        if (storage.TryGetValue(key, out var value))
            return Task.FromResult(Option.Some(value));
        return Task.FromResult<Option<TValue>>(Option.None);
    }

    public Task<Option<TValue>> TryRemove(TKey key, CancellationToken token = default)
    {
        if (storage.TryRemove(key, out var value))
            return Task.FromResult(Option.Some(value));
        return Task.FromResult<Option<TValue>>(Option.None);
    }

    public Task<StorageValue<TValue>> AddOrGet(TKey key, Func<TKey, Task<StorageValue<TValue>>> addFactory, CancellationToken token)
    {
        var value = storage.GetOrAdd(key, _ => addFactory(key).Result.Value);
        return Task.FromResult(new StorageValue<TValue>(value) {CorrelationId = "test", User = "test"});
    }

    public Task<StorageValue<TValue>> AddOrUpdate(TKey key, Func<TKey, Task<StorageValue<TValue>>> addFactory, Func<TKey, StorageValue<TValue>, Task<StorageValue<TValue>>> updateFactory, CancellationToken token)
    {
        var value = storage.AddOrUpdate(
            key,
            _ => addFactory(key).Result.Value,
            (k, v) =>
            {
                var old = new StorageValue<TValue>(v);
                return updateFactory(k, old).Result.Value;
            });
        return Task.FromResult(new StorageValue<TValue>(value) { CorrelationId = "test", User = "test" });
    }

    public Task<Option<StorageValue<TValue>>> TryGetDetailed(TKey key, CancellationToken token) =>
        Task.FromResult(storage.TryGetValue(key, out var value)
            ? Option.Some(new StorageValue<TValue>(value))
            : Option.None);

    public IAsyncEnumerable<TKey> GetKeys(CancellationToken token = default) =>
        storage.Keys.AsAsync();

    public void Add(TKey key, TValue value) => storage.GetOrAdd(key, value);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => storage.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
