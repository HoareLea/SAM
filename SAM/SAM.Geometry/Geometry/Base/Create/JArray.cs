// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Create
    {
        public static JArray JArray(this IEnumerable<ISAMGeometry> sAMGeometries)
        {
            if (sAMGeometries == null)
                return null;

            JArray jArray = new JArray();
            foreach (ISAMGeometry sAMGeometry in sAMGeometries)
                jArray.Add(sAMGeometry.ToJObject());

            return jArray;
        }

        public static JArray JArray<T>(this IEnumerable<T> sAMGeometries) where T : ISAMGeometry
        {
            if (sAMGeometries == null)
                return null;

            JArray jArray = new JArray();
            foreach (T t in sAMGeometries)
                jArray.Add(t.ToJObject());

            return jArray;
        }
    }
}
