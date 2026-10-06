// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SAM.Core;
using Xunit;

namespace SAM.Tests
{
    public class QueryWaitToUnlockTests
    {
        // Generous bound for calls that must return after a handful of ~10 ms attempts; a hang trips it.
        private static readonly TimeSpan Bound = TimeSpan.FromSeconds(10);

        private static string CreateTempFile()
        {
            string path = Path.Combine(Path.GetTempPath(), "SAM.Tests.WaitToUnlock." + Guid.NewGuid().ToString("N") + ".tmp");
            File.WriteAllText(path, "x");
            return path;
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WaitToUnlock_BlankPath_ReturnsFalse(string? path)
        {
            Assert.False(Query.WaitToUnlock(path!, 1, 1));
        }

        [Fact]
        public void WaitToUnlock_MissingFile_ReturnsFalse()
        {
            string path = Path.Combine(Path.GetTempPath(), "SAM.Tests.WaitToUnlock.missing." + Guid.NewGuid().ToString("N"));

            Assert.False(Query.WaitToUnlock(path, 1, 1));
        }

        [Fact]
        public void WaitToUnlock_UnlockedFile_ReturnsTrue()
        {
            string path = CreateTempFile();
            try
            {
                Assert.True(Query.WaitToUnlock(path, 1, 1));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void WaitToUnlock_PersistentlyLockedFile_ReturnsFalseAfterConfiguredAttempts()
        {
            string path = CreateTempFile();
            FileStream holder = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            try
            {
                Task<bool> task = Task.Run(() => Query.WaitToUnlock(path, 10, 3));

                bool completed = task.Wait(Bound);

                // Release first so a hung implementation can end instead of leaking a spinning thread.
                holder.Dispose();

                Assert.True(completed, "WaitToUnlock did not return for a persistently locked file; the retry counter never advances.");
                Assert.False(task.Result);
            }
            finally
            {
                holder.Dispose();
                File.Delete(path);
            }
        }

        [Fact]
        public void WaitToUnlock_FileUnlockedWhileWaiting_ReturnsTrue()
        {
            string path = CreateTempFile();
            FileStream holder = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            try
            {
                Task<bool> task = Task.Run(() => Query.WaitToUnlock(path, 20, 200));

                Thread.Sleep(100);
                holder.Dispose();

                Assert.True(task.Wait(Bound));
                Assert.True(task.Result);
            }
            finally
            {
                holder.Dispose();
                File.Delete(path);
            }
        }
    }
}
