using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Face3Ds(this IEnumerable<ISAMGeometry3D> geometry3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (geometry3Ds == null)
                return null;

            List<Face3D> result = new List<Face3D>();
            foreach (ISAMGeometry3D geometry3D in geometry3Ds)
            {
                if (geometry3D is Segment3D)
                    continue;

                if(geometry3D is Shell)
                {
                    List<Face3D> face3Ds = ((Shell)geometry3D).Face3Ds;
                    if(face3Ds != null && face3Ds.Count > 0)
                    {
                        result.AddRange(face3Ds);
                        continue;
                    }
                }

                if (geometry3D is Face3D)
                {
                    result.Add((Face3D)geometry3D);
                    continue;
                }

                if (geometry3D is IClosedPlanar3D)
                {
                    IClosedPlanar3D closedPlanar3D = (IClosedPlanar3D)geometry3D;
                    Plane plane = closedPlanar3D.GetPlane();
                    if (plane == null)
                        continue;

                    result.Add(new Face3D(closedPlanar3D));
                    continue;
                }

                if (geometry3D is ICurvable3D)
                {
                    List<Point3D> point3Ds = ((ICurvable3D)geometry3D).GetCurves().ConvertAll(x => x.GetStart());
                    result.Add(new Face3D(new Polygon3D(point3Ds, tolerance)));
                }
            }

            return result;
        }

        public static List<Face3D> Face3Ds(this Extrusion extrusion, double tolerance = Core.Tolerance.Distance)
        {
            return Create.Shell(extrusion, tolerance)?.Face3Ds;
        }
    }
}