// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Rhino;

namespace SAM.Analytical.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Brep ToRhino(this PlanarBoundary3D planarBoundary3D)
        {
            return Geometry.Rhino.Convert.ToRhino_Brep(planarBoundary3D?.GetFace3D());
        }

        public static global::Rhino.Geometry.Brep ToRhino(this IPanel panel, double tolerance = Core.Tolerance.Distance)
        {
            global::Rhino.Geometry.Brep result = panel.Face3D.ToRhino_Brep(tolerance);
            if(result is null)
            {
                return null;
            }

            string json = Core.Convert.ToString(panel);
            if(!string.IsNullOrWhiteSpace(json))
            {
                result.SetUserString(Core.Rhino.Names.UserString, json);
            }

            return result;
        }

        public static global::Rhino.Geometry.Brep ToRhino(this Aperture aperture, double tolerance = Core.Tolerance.Distance)
        {
            global::Rhino.Geometry.Brep result = aperture.Face3D.ToRhino_Brep(tolerance);
            if (result is null)
            {
                return null;
            }

            string json = Core.Convert.ToString(aperture);
            if (!string.IsNullOrWhiteSpace(json))
            {
                result.SetUserString(Core.Rhino.Names.UserString, json);
            }

            return result;
        }
    }
}
