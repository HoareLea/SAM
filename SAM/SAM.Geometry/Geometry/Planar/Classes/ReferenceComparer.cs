// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SAM.Geometry.Planar
{
    /// <summary>
    /// Object identity, ignoring any Equals override. For the operation-local sets that track
    /// which instances a de-duplication pass has already kept.
    /// <para>
    /// <c>Query.AlmostSimilar</c> answers true for the same instance before it consults the
    /// tolerance, so an instance is similar to itself at every tolerance - including a negative
    /// one, where nothing else is similar to anything. A geometric broad-phase filter has no
    /// way to express that, so identity has to be tracked alongside the spatial index rather
    /// than inferred from it.
    /// </para>
    /// <para>
    /// Value types boxed into <see cref="object"/> are never identical to a separate boxing of
    /// the same value, which is exactly how the interface-typed <c>==</c> in AlmostSimilar
    /// behaves for them. The two agree without a special case.
    /// </para>
    /// </summary>
    internal sealed class ReferenceComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceComparer Instance = new ReferenceComparer();

        private ReferenceComparer()
        {
        }

        public new bool Equals(object object_1, object object_2)
        {
            return ReferenceEquals(object_1, object_2);
        }

        public int GetHashCode(object @object)
        {
            return RuntimeHelpers.GetHashCode(@object);
        }
    }
}
