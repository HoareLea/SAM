using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static List<LinkedFace3D> LinkedFace3Ds(this LinkedFace3D solarFace, Vector3D vector3D, IEnumerable<Polygon2D> polygon2Ds, Plane plane, double tolerance = Core.Tolerance.Distance)
        {
            List<Polygon2D> polygon2Ds_SolarFace = Planar.Query.Union(polygon2Ds, tolerance);
            if (polygon2Ds_SolarFace == null || polygon2Ds_SolarFace.Count == 0)
            {
                return null;
            }

            Face3D face3D_SolarFace = solarFace.Face3D;
            Plane plane_SolarFace = face3D_SolarFace.GetPlane();
            Face2D face2D = plane_SolarFace.Convert(face3D_SolarFace);

            List<LinkedFace3D> result = new List<LinkedFace3D>();
            foreach (Polygon2D polygon2D_SolarFace in polygon2Ds_SolarFace)
            {
                Polygon3D polygon3D_Shade = plane.Convert(polygon2D_SolarFace);

                polygon3D_Shade = plane_SolarFace.Project(polygon3D_Shade, vector3D, tolerance);
                if (polygon3D_Shade == null || !polygon3D_Shade.IsValid())
                {
                    continue;
                }

                Polygon2D polygon2D_Shade = plane_SolarFace.Convert(polygon3D_Shade);
                if (polygon2D_Shade == null)
                {
                    continue;
                }

                List<Face2D> face2Ds_Shade = Planar.Query.Intersection(face2D, new Face2D(polygon2D_Shade));
                if (face2Ds_Shade == null || face2Ds_Shade.Count == 0)
                {
                    continue;
                }

                foreach (Face2D face2D_Shade in face2Ds_Shade)
                {
                    result.Add(new LinkedFace3D(solarFace.Guid, plane_SolarFace.Convert(face2D_Shade)));
                }
            }

            return result;
        }
    }
}