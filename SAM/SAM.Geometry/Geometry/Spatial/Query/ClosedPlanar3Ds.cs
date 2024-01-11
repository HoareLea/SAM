using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<IClosedPlanar3D> ClosedPlanar3Ds(this IEnumerable<ISAMGeometry3D> geometry3Ds)
        {
            if (geometry3Ds == null)
                return null;

            List<IClosedPlanar3D> result = new List<IClosedPlanar3D>();
            foreach (ISAMGeometry3D geometry3D in geometry3Ds)
            {
                if (geometry3D is Segment3D)
                    continue;

                if (geometry3D is Face3D)
                {
                    result.AddRange(((Face3D)geometry3D).GetEdge3Ds());
                    continue;
                }

                if (geometry3D is IClosedPlanar3D)
                {
                    result.Add((IClosedPlanar3D)geometry3D);
                    continue;
                }

                if (geometry3D is ICurvable3D)
                {
                    List<ICurve3D> curve3Ds = ((ICurvable3D)geometry3D).GetCurves();
                    Point3D point3D_Start = curve3Ds.First()?.GetStart();
                    Point3D point3D_End = curve3Ds.Last()?.GetEnd();
                    if (point3D_Start == null || point3D_End == null)
                        continue;

                    if (!point3D_Start.AlmostEquals(point3D_End))
                        continue;
                    
                    List<Point3D> point3Ds = ((ICurvable3D)geometry3D).GetCurves().ConvertAll(x => x.GetStart());
                    result.Add(new Polygon3D(point3Ds));
                }
            }

            return result;
        }
    }
}