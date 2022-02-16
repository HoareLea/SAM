using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static List<LinkedFace3D> LinkedFace3Ds(this LinkedFace3D solarFace, Vector3D vector3D, IEnumerable<Face2D> face2Ds, Plane plane, double tolerance = Core.Tolerance.Distance)
        {
            List<Face2D> face2Ds_SolarFace = Planar.Query.Union(face2Ds, tolerance);
            if (face2Ds_SolarFace == null || face2Ds_SolarFace.Count == 0)
            {
                return null;
            }

            Face3D face3D_SolarFace = solarFace.Face3D;
            Plane plane_SolarFace = face3D_SolarFace.GetPlane();
            Face2D face2D = plane_SolarFace.Convert(face3D_SolarFace);

            List<LinkedFace3D> result = new List<LinkedFace3D>();
            foreach (Face2D face2D_SolarFace in face2Ds_SolarFace)
            {
                Face3D face3D_Shade = plane.Convert(face2D_SolarFace);

                face3D_Shade = plane_SolarFace.Project(face3D_Shade, vector3D, tolerance);
                if (face3D_Shade == null || !face3D_Shade.IsValid())
                {
                    continue;
                }

                Face2D face2D_Shade = plane_SolarFace.Convert(face3D_Shade);
                if (face2D_Shade == null)
                {
                    continue;
                }

                List<Face2D> face2Ds_Shade = Planar.Query.Intersection(face2D, face2D_Shade);
                if (face2Ds_Shade == null || face2Ds_Shade.Count == 0)
                {
                    continue;
                }

                foreach (Face2D face2D_Shade_Temp in face2Ds_Shade)
                {
                    result.Add(new LinkedFace3D(solarFace.Guid, plane_SolarFace.Convert(face2D_Shade_Temp)));
                }
            }

            return result;
        }
    }
}