using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PQCons.Tasks
{
    public class LockByKey<TKey> where TKey : IEquatable<TKey>
    {
        public int LockCount => _locks.Count;
        private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks;

        public LockByKey() => _locks = new ConcurrentDictionary<TKey, SemaphoreSlim>();

        public LockByKey(IEqualityComparer<TKey> equalityComparer)
            => _locks = new ConcurrentDictionary<TKey, SemaphoreSlim>(equalityComparer);

        public async Task<IDisposable> AcquireLockAsync(TKey key)
            => await GetReleaserAsync(key).ConfigureAwait(false);

        private async Task<IDisposable> GetReleaserAsync(TKey key)
        {
            var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync().ConfigureAwait(false);
            return new LockByKeyReleaser<TKey>(key, semaphore, _locks);
        }
    }
}