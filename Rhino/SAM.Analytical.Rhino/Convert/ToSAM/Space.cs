// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Rhino;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Rhino
{
    public static partial class Convert
    {
        public static Space ToSAM_Space(this global::Rhino.Geometry.Point point)
        {
            if (point is null)
            {
                return null;
            }

            Space result = Query.AnalyticalObject<Space>(point);
            if (result is null)
            {
                result = new Space(System.Guid.NewGuid().ToString(), point.ToSAM());
            }
            else
            {
                result = new Space(result.Guid, result, result.Name, point.ToSAM());
            }

            return result;
        }
    }
}
