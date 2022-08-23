using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Between(this Plane plane_1, Plane plane_2, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane_1 == null || plane_2 == null || point3D == null)
                return false;

            return (plane_1.Above(point3D, tolerance) && plane_2.Below(point3D, tolerance)) || (plane_2.Above(point3D, tolerance) && plane_1.Below(point3D, tolerance));
        }

        public static List<Face3D> Between(this IEnumerable<Face3D> face3Ds, double elevation_1, double elevation_2, double tolerance = Core.Tolerance.Distance)
        {
            double min = System.Math.Min(elevation_1, elevation_2);
            double max = System.Math.Max(elevation_1, elevation_2);

            Plane plane_Bottom = Create.Plane(min);
            Plane plane_Top = Create.Plane(max);

            List<Face3D> result = new List<Face3D>();

            foreach (Face3D face3D in face3Ds)
            {
                List<Face3D> face3Ds_Cut = face3D.Cut(new Plane[] { plane_Top, plane_Bottom }, tolerance);
                if (face3Ds_Cut == null || face3Ds_Cut.Count == 0)
                {
                    continue;
                }

                foreach(Face3D face3D_Cut in face3Ds_Cut)
                {
                    BoundingBox3D boundingBox3D = face3D_Cut?.GetBoundingBox();
                    if(boundingBox3D == null)
                    {
                        continue;
                    }

                    double value;

                    value = boundingBox3D.Min.Z;
                    if(Core.Query.AlmostEqual(value, min) || value >= min)
                    {
                        value = boundingBox3D.Max.Z;
                        if (Core.Query.AlmostEqual(value, max) || value <= max)
                        {
                            result.Add(face3D_Cut);
                        }
                    }
                }
            }

            return result;
        }
    }
}