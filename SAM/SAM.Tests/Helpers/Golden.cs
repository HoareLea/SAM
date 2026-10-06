// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.IO;
using Xunit;

namespace SAM.Tests.Helpers
{
    /// <summary>
    /// Golden-file snapshots. Goldens live in <c>SAM.Tests/Golden</c> and are copied to the test output.
    /// <para>
    /// To accept a deliberate change, run the tests with <c>SAM_UPDATE_GOLDEN=1</c>: the current output is written
    /// back to the source folder (found by walking up from the test output to <c>SAM.Tests.csproj</c>), and the
    /// test still fails once so the update is never silent. Review the diff before committing it.
    /// </para>
    /// </summary>
    public static class Golden
    {
        public static void AssertMatches(string fileName, string actual)
        {
            actual = Normalize(actual);

            string path = Path.Combine(AppContext.BaseDirectory, "Golden", fileName);
            string expected = File.Exists(path) ? Normalize(File.ReadAllText(path)) : null;

            if (expected == actual)
            {
                return;
            }

            if (Environment.GetEnvironmentVariable("SAM_UPDATE_GOLDEN") == "1")
            {
                string directory = SourceDirectory();
                Assert.NotNull(directory);
                File.WriteAllText(Path.Combine(directory, "Golden", fileName), actual);
                Assert.Fail(string.Format("Golden {0} updated; review the diff and re-run.", fileName));
            }

            Assert.True(expected != null, string.Format("Golden {0} is missing. Run with SAM_UPDATE_GOLDEN=1 to create it.", fileName));
            Assert.Equal(expected, actual);
        }

        private static string Normalize(string text)
        {
            return text.Replace("\r\n", "\n").TrimEnd('\n') + "\n";
        }

        private static string SourceDirectory()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(AppContext.BaseDirectory);
            while (directoryInfo != null)
            {
                if (File.Exists(Path.Combine(directoryInfo.FullName, "SAM.Tests.csproj")))
                {
                    return directoryInfo.FullName;
                }

                directoryInfo = directoryInfo.Parent;
            }

            return null;
        }
    }
}
