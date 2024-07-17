using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PQCons.Tasks
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class LockByKey<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// The current number of locks.
        /// </summary>
        public int LockCount => _locks.Count;
        private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks;

        /// <summary>
        /// Constructor using the default comparer of TKey.
        /// </summary>
        public LockByKey() => _locks = new ConcurrentDictionary<TKey, SemaphoreSlim>();

        /// <summary>
        /// Constructor specifying an IEqualityComparer to use for the TKey.
        /// </summary>
        /// <param name="equalityComparer"></param>
        /// <example>new LockByKey&lt;string&gt;(StringComparer.OrdinalIgnoreCase)</example>
        public LockByKey(IEqualityComparer<TKey> equalityComparer)
            => _locks = new ConcurrentDictionary<TKey, SemaphoreSlim>(equalityComparer);

        /// <summary>
        /// Start a keyed critical section.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>IDisposable object that releases the lock on dispose</returns>
        /// <example>using (await _locks.AcquireLockAsync("key")) { ... }</example>
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