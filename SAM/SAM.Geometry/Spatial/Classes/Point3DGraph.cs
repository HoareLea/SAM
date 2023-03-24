using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public class Point3DGraph<T> : PointGraph<Point3D, T> where T : IJSAMObject
    {
        public Point3DGraph()
            : base()
        {

        }

        public Point3DGraph(double tolerance)
            :base(tolerance)
        {

        }

        public Point3DGraph(JObject jObject)
            :base(jObject)
        {

        }

        public Point3DGraph(Point3DGraph<T> point3DGraph)
            :base(point3DGraph)
        {
      
        }

        public List<Point3DGraph<T>> Split()
        {
            return Split<Point3DGraph<T>>();
        }

        public override Point3D Find(Point3D point3D)
        {
            if (point3D == null)
            {
                return null;
            }

            List<Point3D> point3Ds = GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
            {

            }

            foreach (Point3D point3D_Temp in point3Ds)
            {
                if (point3D.AlmostEquals(point3D_Temp, Tolerance))
                {
                    return point3D_Temp;
                }
            }

            return null;
        }

        protected override double Weight(PointGraphEdge<Point3D, T> pointGraphEdge)
        {
            return pointGraphEdge.Source.Distance(pointGraphEdge.Target);
        }
    }
}