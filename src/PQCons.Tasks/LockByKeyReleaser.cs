using System.Collections.Concurrent;

namespace PQCons.Tasks;

internal class LockByKeyReleaser<TKey>(TKey key, SemaphoreSlim semaphore,
    ConcurrentDictionary<TKey, SemaphoreSlim> locks) : IDisposable where TKey : IEquatable<TKey>
{
    private readonly SemaphoreSlim _semaphore = semaphore;
    private readonly TKey _key = key;
    private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks = locks;

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