// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Gets External BoundingBox3Ds for given boundingBox3Ds
        /// </summary>
        /// <param name="boundingBox3Ds">BoundingBox3Ds</param>
        /// <returns>External BoundingBox3Ds</returns>
        public static List<BoundingBox3D> External(IEnumerable<BoundingBox3D> boundingBox3Ds)
        {
            if (boundingBox3Ds == null)
            {
                return null;
            }

            List<BoundingBox3D> list = boundingBox3Ds as List<BoundingBox3D> ?? new List<BoundingBox3D>(boundingBox3Ds);
            int count = list.Count;
            if (count == 0)
            {
                return new List<BoundingBox3D>();
            }

            if (count == 1)
            {
                return new List<BoundingBox3D>() { list[0] };
            }

            HashSet<int> indexes = new HashSet<int>();

            for (int i = 0; i < count - 1; i++)
            {
                if (indexes.Contains(i))
                {
                    continue;
                }

                BoundingBox3D boundingBox3D_1 = list[i];
                if (boundingBox3D_1 == null)
                {
                    continue;
                }

                for (int j = 0; j < count; j++)
                {
                    if (i == j || indexes.Contains(j))
                    {
                        continue;
                    }

                    BoundingBox3D boundingBox3D_2 = list[j];
                    if (boundingBox3D_2 == null)
                    {
                        continue;
                    }

                    if (boundingBox3D_1.Inside(boundingBox3D_2))
                    {
                        indexes.Add(j);
                    }
                }
            }

            List<BoundingBox3D> result = new List<BoundingBox3D>();
            for (int i = 0; i < count; i++)
            {
                if (indexes.Contains(i))
                {
                    continue;
                }

                result.Add(list[i]);
            }

            return result;
        }
    }
}
