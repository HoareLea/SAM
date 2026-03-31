// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Rhino;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Convert
    {
        public static List<Aperture> ToSAM_Apertures(this global::Rhino.Geometry.Brep brep, double tolerance = Core.Tolerance.Distance)
        {
            if(brep is null)
            {
                return null;
            }

            Aperture aperture = Query.AnalyticalObject<Aperture>(brep);

            List<Face3D> face3Ds = brep.ToSAM(true, true, tolerance)?.FindAll(x => x is Face3D).ConvertAll(x => (Face3D)x);
            if(face3Ds is null)
            {
                return null;
            }

            List<Aperture> result = new List<Aperture>();

            for (int i = 0; i < face3Ds.Count; i++)
            {
                Aperture aperture_Temp = aperture;


                if (aperture_Temp == null)
                {
                    aperture_Temp = Create.Aperture(null, face3Ds[0]);
                }
                else
                {
                    aperture_Temp = Create.Aperture(aperture, face3Ds[0], i == 0 ? aperture.Guid : System.Guid.NewGuid());
                }



                if (aperture_Temp.ApertureConstruction == null)
                {
                    Vector3D normal = aperture_Temp?.Face3D?.GetPlane()?.Normal;
                    if(normal is null)
                    {
                        ApertureConstruction apertureConstruction_Temp = Analytical.Query.DefaultApertureConstruction(Analytical.Query.PanelType(normal), aperture_Temp.ApertureType);
                        if (apertureConstruction_Temp != null)
                        {
                            aperture_Temp = new Aperture(aperture_Temp, apertureConstruction_Temp);
                        }
                    }
                }

                result.Add(aperture_Temp);
            }

            return result;
        }
    }
}
