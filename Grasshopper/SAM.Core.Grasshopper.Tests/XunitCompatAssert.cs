// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper.Tests
{
    /// <summary>
    /// xUnit-shaped Assert/Skip surface backed by NUnit. The test classes in this project were
    /// originally written against xUnit + Xunit.SkippableFact; moving the project onto NUnit +
    /// Rhino.Testing (the only way to host a real Rhino/Grasshopper runtime under dotnet test)
    /// would otherwise force a rewrite of every assertion. Since this namespace is searched
    /// before "using NUnit.Framework", unqualified Assert/Skip calls below resolve here, not to
    /// NUnit.Framework.Assert - so the test bodies themselves needed no changes.
    /// </summary>
    internal static class Assert
    {
        public static void True(bool condition) => NUnit.Framework.Assert.That(condition, NUnit.Framework.Is.True);

        public static void False(bool condition) => NUnit.Framework.Assert.That(condition, NUnit.Framework.Is.False);

        public static void Null(object value) => NUnit.Framework.Assert.That(value, NUnit.Framework.Is.Null);

        public static void NotNull(object value) => NUnit.Framework.Assert.That(value, NUnit.Framework.Is.Not.Null);

        public static void Same(object expected, object actual) => NUnit.Framework.Assert.That(actual, NUnit.Framework.Is.SameAs(expected));

        public static void Equal<T>(T expected, T actual) => NUnit.Framework.Assert.That(actual, NUnit.Framework.Is.EqualTo(expected));

        public static void NotEqual<T>(T expected, T actual) => NUnit.Framework.Assert.That(actual, NUnit.Framework.Is.Not.EqualTo(expected));

        public static void Empty(IEnumerable collection) => NUnit.Framework.Assert.That(collection, NUnit.Framework.Is.Empty);

        public static void NotEmpty(IEnumerable collection) => NUnit.Framework.Assert.That(collection, NUnit.Framework.Is.Not.Empty);

        public static T Single<T>(IEnumerable<T> collection)
        {
            List<T> list = collection.ToList();
            NUnit.Framework.Assert.That(list, NUnit.Framework.Has.Count.EqualTo(1));
            return list[0];
        }

        public static void Contains<T>(T expected, IEnumerable<T> collection) => NUnit.Framework.Assert.That(collection, NUnit.Framework.Does.Contain(expected));

        public static void Contains(string expectedSubstring, string actualString) => NUnit.Framework.Assert.That(actualString, NUnit.Framework.Does.Contain(expectedSubstring));

        public static void StartsWith(string expectedStart, string actualString) => NUnit.Framework.Assert.That(actualString, NUnit.Framework.Does.StartWith(expectedStart));
    }

    internal static class Skip
    {
        public static void If(bool condition, string reason)
        {
            if (condition)
            {
                NUnit.Framework.Assert.Ignore(reason);
            }
        }
    }
}
