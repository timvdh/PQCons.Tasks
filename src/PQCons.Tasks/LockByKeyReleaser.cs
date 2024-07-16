using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PQCons.Tasks
{
    internal class LockByKeyReleaser<TKey> : IDisposable where TKey : IEquatable<TKey>
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly TKey _key;
        private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks;

        public LockByKeyReleaser(TKey key, SemaphoreSlim semaphore,
            ConcurrentDictionary<TKey, SemaphoreSlim> locks)
        {
            _semaphore = semaphore;
            _key = key;
            _locks = locks;
        }

        public void Dispose()
        {
            _semaphore.Release();
            if (_semaphore.CurrentCount == 1) // No one else is waiting, we can remove the semaphore.
            {
                _locks.TryRemove(_key, out _);
                _semaphore.Dispose();
            }
        }
    }
}