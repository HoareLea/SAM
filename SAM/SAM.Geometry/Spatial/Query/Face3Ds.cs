using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Face3Ds(this IEnumerable<ISAMGeometry3D> geometry3Ds)
        {
            if (geometry3Ds == null)
                return null;

            List<Face3D> faces = new List<Face3D>();
            foreach (ISAMGeometry3D geometry3D in geometry3Ds)
            {
                if (geometry3D is Segment3D)
                    continue;

                if (geometry3D is Face3D)
                {
                    faces.Add((Face3D)geometry3D);
                    continue;
                }

                if (geometry3D is IClosedPlanar3D)
                {
                    IClosedPlanar3D closedPlanar3D = (IClosedPlanar3D)geometry3D;
                    Plane plane = closedPlanar3D.GetPlane();
                    if (plane == null)
                        continue;

                    faces.Add(new Face3D(closedPlanar3D));
                    continue;
                }

                if (geometry3D is ICurvable3D)
                {
                    List<Point3D> point3Ds = ((ICurvable3D)geometry3D).GetCurves().ConvertAll(x => x.GetStart());
                    faces.Add(new Face3D(new Polygon3D(point3Ds)));
                }
            }

            return faces;
        }
    }
}