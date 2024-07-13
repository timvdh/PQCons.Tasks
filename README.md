# PQCons.Tasks

A simple library for task management.

## LockByKey

A utility class providing keyed locks. Useful, if we need to control single task access to a resource (which is represented by TKey).

Features:
- Stores locks in a dictionary and removes entries once there are no more waiting semaphores.
- Can use the same IEqualityComparer<> as ConcurrentDictionary<>.
- Does not guarantee execution order (no FIFO).
- Does not limit the overall amount of tasks.

### Basic Usage

```csharp
using (await sut.AcquireLockAsync("key"))
{
    // this is the critical section to be locked by "key".
}
```

Check tests or PQCons.Tasks.Samples for more examples.