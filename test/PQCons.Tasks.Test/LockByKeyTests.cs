using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PQCons.Tasks.Tests
{
    [TestClass()]
    public class LockByKeyTests
    {
        [TestMethod()]
        public async Task LockByKey_semaphore_immediately_released()
        {
            var sut = new LockByKey<string>();
            using (await sut.AcquireLockAsync("key"))
            {
                Assert.IsTrue(sut.LockCount == 1);
            }
            Assert.IsTrue(sut.LockCount == 0);
        }

        [TestMethod()]
        public async Task LockByKey_semaphore_immediately_released_with_nested_blocks()
        {
            var sut = new LockByKey<string>();
            using (await sut.AcquireLockAsync("key1"))
            {
                using (await sut.AcquireLockAsync("key2"))
                {
                    Assert.IsTrue(sut.LockCount == 2);
                }
                Assert.IsTrue(sut.LockCount == 1);
            }
            Assert.IsTrue(sut.LockCount == 0);
        }

        [TestMethod()]
        public async Task LockByKey_string_key()
        {
            await CheckInUseWithTenTasks("key");
        }

        [TestMethod()]
        public async Task LockByKey_int_key()
        {
            await CheckInUseWithTenTasks(0);
        }

        private static async Task CheckInUseWithTenTasks<TKey>(TKey key) where TKey : IEquatable<TKey>
        {
            InUseCheck inUseCheck = new InUseCheck();

            var sut = new LockByKey<TKey>();
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(LockAndCheckInUse(sut, key, inUseCheck));
            }
            await Task.WhenAll(tasks);
        }

        private static async Task LockAndCheckInUse<TKey>(LockByKey<TKey> sut,
                                                   TKey key,
                                                   InUseCheck inUseCheck) where TKey : IEquatable<TKey>
        {
            using (await sut.AcquireLockAsync(key))
            {
                Assert.IsFalse(inUseCheck.InUse);
                inUseCheck.InUse = true;
                await Task.Delay(100);
                inUseCheck.InUse = false;
                Assert.IsFalse(inUseCheck.InUse);
            }
        }

        private class InUseCheck
        {
            public bool InUse { get; set; } = false;
        }
    }
}