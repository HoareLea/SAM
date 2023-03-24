using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class Point2DGraph<T> : PointGraph<Point2D, T> where T : IJSAMObject
    {
        public Point2DGraph()
            : base()
        {

        }

        public Point2DGraph(double tolerance)
            :base(tolerance)
        {

        }

        public Point2DGraph(JObject jObject)
            :base(jObject)
        {

        }

        public Point2DGraph(Point2DGraph<T> point2DGraph)
            :base(point2DGraph)
        {
      
        }

        public List<Point2DGraph<T>> Split()
        {
            return base.Split<Point2DGraph<T>>();
        }

        public override Point2D Find(Point2D point2D)
        {
            if (point2D == null)
            {
                return null;
            }

            List<Point2D> point2Ds = GetPoints();
            if (point2Ds == null || point2Ds.Count == 0)
            {

            }

            foreach (Point2D point2D_Temp in point2Ds)
            {
                if (point2D.AlmostEquals(point2D_Temp, Tolerance))
                {
                    return point2D_Temp;
                }
            }

            return null;
        }

        protected override double Weight(PointGraphEdge<Point2D, T> pointGraphEdge)
        {
            return pointGraphEdge.Source.Distance(pointGraphEdge.Target);
        }
    }
}