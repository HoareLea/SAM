// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Rhino
{
    public static partial class Convert
    {
        public static List<IAnalyticalObject> ToSAM(this RhinoObject rhinoObject)
        {
            if(rhinoObject is null)
            {
                return null;
            }

            if(rhinoObject.Geometry is Point point)
            {
                Space space = ToSAM_Space(point);
                if(space is null)
                {
                    return null;
                }

                return new List<IAnalyticalObject>() { space };
            }

            if (rhinoObject.Geometry is Brep brep)
            {
                Panel panel = Query.AnalyticalObject<Panel>(brep);
                if(panel != null)
                {
                    return ToSAM_Panels(brep)?.Cast<IAnalyticalObject>().ToList();
                }

                Aperture aperture = Query.AnalyticalObject<Aperture>(brep);
                if (aperture != null)
                {
                    return ToSAM_Apertures(brep)?.Cast<IAnalyticalObject>().ToList();
                }
            }

            return null;
        }

        public static List<IAnalyticalObject> ToSAM(this IEnumerable<RhinoObject> rhinoObjects)
        {
            if (rhinoObjects is null)
            {
                return null;
            }

            List<IAnalyticalObject> result = new List<IAnalyticalObject>();
            foreach(RhinoObject rhinoObject in rhinoObjects)
            {
                List<IAnalyticalObject> analyticalObjects = rhinoObject.ToSAM();
                if(rhinoObjects is null)
                {
                    continue;
                }

                result.AddRange(analyticalObjects);
            }

            return result;
        }
    }
}
