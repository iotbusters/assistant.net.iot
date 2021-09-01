using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Mocks
{
    public class TestStorage<TKey, TValue> : IStorage<TKey, TValue>, IEnumerable<KeyValuePair<TKey,TValue>>
        where TKey : struct
    {
        private readonly ConcurrentDictionary<TKey, TValue> storage = new();

        public Task<TValue> AddOrGet(TKey key, Func<TKey, Task<TValue>> addFactory) =>
            Task.FromResult(storage.GetOrAdd(key, _ => addFactory(key).Result));

        public Task<TValue> AddOrUpdate(TKey key, Func<TKey, Task<TValue>> addFactory, Func<TKey, TValue, Task<TValue>> updateFactory) =>
            Task.FromResult(storage.AddOrUpdate(key, _ => addFactory(key).Result, (k, v) => updateFactory(k, v).Result));

        public Task<Option<TValue>> TryGet(TKey key)
        {
            if (storage.TryGetValue(key, out var value))
                return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<TValue>>(Option.None);
        }

        public Task<Option<TValue>> TryRemove(TKey key)
        {
            if (storage.TryRemove(key, out var value))
                return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<TValue>>(Option.None);
        }

        public IAsyncEnumerable<TKey> GetKeys() =>
            storage.Keys.AsAsync();

        public void Add(TKey key, TValue value) => storage.GetOrAdd(key, value);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => storage.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}