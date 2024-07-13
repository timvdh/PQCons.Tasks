using PQCons.Tasks.Samples;

Console.WriteLine("Testing without locking...");
var withoutLock = new LockByKeySample(useLocking: false);
await withoutLock.TestParallel();
Console.WriteLine($"...done in {withoutLock.Stopwatch.ElapsedMilliseconds} milliseconds.");

Console.WriteLine();

Console.WriteLine("Testing with locking...");
var withLock = new LockByKeySample(useLocking: true);
await withLock.TestParallel();
Console.WriteLine($"...done in {withLock.Stopwatch.ElapsedMilliseconds} milliseconds.");
