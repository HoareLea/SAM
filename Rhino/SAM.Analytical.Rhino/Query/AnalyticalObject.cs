// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Rhino.Geometry;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Query
    {
        public static TAnalyticalObject AnalyticalObject<TAnalyticalObject>(this GeometryBase geometryBase) where TAnalyticalObject : IAnalyticalObject
        {
            if(geometryBase is null)
            {
                return default;
            }

            string json = geometryBase.GetUserString(Core.Rhino.Names.UserString);
            if (!string.IsNullOrWhiteSpace(json))
            {
                List<TAnalyticalObject> apertures = Core.Convert.ToSAM<TAnalyticalObject>(json);
                if (apertures != null && apertures.Count != 0)
                {
                    return apertures[0];
                }
            }

            return default;
        }
    }
}
