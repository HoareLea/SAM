// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static List<GH_Surface> ToGrasshopper(this Aperture aperture, bool includeFrame = false)
        {
            if (aperture == null)
            {
                return null;
            }

            List<GH_Surface> surfaces = new List<GH_Surface>();

            if (!includeFrame)
            {
                surfaces.Add(new GH_Surface(Geometry.Rhino.Convert.ToRhino_Brep(new Face3D(aperture.GetExternalEdge3D()))));
            }
            else
            {
                List<Face3D> face3Ds;

                face3Ds = aperture.GetFace3Ds(AperturePart.Frame);
                face3Ds?.ForEach(x => new GH_Surface(Geometry.Rhino.Convert.ToRhino_Brep(x)));

                face3Ds = aperture.GetFace3Ds(AperturePart.Pane);
                face3Ds?.ForEach(x => new GH_Surface(Geometry.Rhino.Convert.ToRhino_Brep(x)));
            }

            return surfaces;
        }

        public static List<GH_Surface> ToGrasshopper(this Panel panel, bool cutApertures = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Face3D> face3Ds = panel?.GetFace3Ds(cutApertures);
            if (face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            List<GH_Surface> result = new List<GH_Surface>();
            foreach (Face3D face3D in face3Ds)
            {
                GH_Surface surface = face3D?.ToGrasshopper(tolerance);
                if (surface == null)
                {
                    continue;
                }

                result.Add(surface);


            }

            return result;
        }
    }
}
