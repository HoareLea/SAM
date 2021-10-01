using SAM.Core;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polyline2D> Cut(this Polyline2D polyline2D, Point2D point2D_1, Point2D point2D_2)
        {
            if (polyline2D == null || point2D_1 == null || point2D_2 == null)
                return null;

            Point2D point2D_1_Closest = polyline2D.Closest(point2D_1, true);
            Point2D point2D_2_Closest = polyline2D.Closest(point2D_2, true);
            if (point2D_1_Closest.Equals(point2D_2_Closest))
            {
                Polyline2D polyline2D_Temp = new Polyline2D(polyline2D);
                polyline2D_Temp.InsertClosest(point2D_1_Closest);
                int index = polyline2D_Temp.IndexOfClosestPoint2D(point2D_1);
                //polyline2D_Temp.Reorder(index); //DODO: Check if this line is necessary

                return new List<Polyline2D>() { polyline2D_Temp };
            }

            double parameter_1 = polyline2D.GetParameter(point2D_1_Closest);
            double parameter_2 = polyline2D.GetParameter(point2D_2_Closest);

            if (parameter_1 > parameter_2)
            {
                Point2D point2D = point2D_1_Closest;
                point2D_1_Closest = point2D_2_Closest;
                point2D_2_Closest = point2D;

                double parameter = parameter_1;
                parameter_1 = parameter_2;
                parameter_2 = parameter;
            }

            List<Polyline2D> result = new List<Polyline2D>();

            if (parameter_1 == 0)
            {
                if (1 - parameter_2 < parameter_2)
                {
                    result.Add(polyline2D.Trim(parameter_2) as Polyline2D);
                    return result;
                }

                Polyline2D polyline2D_Temp = new Polyline2D(polyline2D);
                polyline2D_Temp.Reverse();
                polyline2D_Temp = polyline2D_Temp.Trim(1 - parameter_2) as Polyline2D;
                polyline2D_Temp.Reverse();

                result.Add(polyline2D_Temp);
                return result;
            }

            result.Add(polyline2D.Trim(parameter_1) as Polyline2D);

            Polyline2D polyline2D_Reversed = new Polyline2D(polyline2D);
            polyline2D_Reversed.Reverse();

            if (parameter_2 != 1)
                result.Add(polyline2D_Reversed.Trim(1 - parameter_2) as Polyline2D);

            return result;
        }

        public static List<Face2D> Cut(this Face2D face2D, IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Tolerance.Distance)
        {
            if (face2D == null || segmentable2Ds == null)
                return null;

            List<IClosed2D> edges = face2D.Edge2Ds;
            if (edges == null || edges.Count == 0)
                return null;

            List<ISegmentable2D> segmentable2Ds_All = new List<ISegmentable2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                    continue;

                segmentable2Ds_All.Add(segmentable2D);
            }

            HashSet<Point2D> point2Ds = new HashSet<Point2D>();
            foreach (IClosed2D closed2D in edges)
            {
                ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
                if (segmentable2D == null)
                    continue;

                segmentable2Ds_All.Add(segmentable2D);
                foreach (Point2D point2D in segmentable2D.GetPoints())
                    point2Ds.Add(point2D);
            }

            if (segmentable2Ds_All == null || segmentable2Ds_All.Count == 0)
                return null;

            List<Segment2D> segment2Ds = segmentable2Ds_All.Split(tolerance);

            List<Polygon2D> polygon2Ds = Create.Polygon2Ds(segment2Ds, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;

            List<IClosed2D> externalEdges_New = new List<IClosed2D>();
            List<IClosed2D> internalEdges_New = new List<IClosed2D>();
            for (int i = 0; i < polygon2Ds.Count; i++)
            {
                Polygon2D polygon2D = Snap(polygon2Ds[i], point2Ds, tolerance);

                Point2D point2D = polygon2D.GetInternalPoint2D(tolerance);
                if (face2D.Inside(point2D, tolerance))
                    externalEdges_New.Add(polygon2D);

                IClosed2D closed2D = internalEdges?.Find(x => x.Inside(point2D, tolerance));
                if (closed2D != null)
                    internalEdges_New.Add(closed2D);
            }

            List<Face2D> result = new List<Face2D>();
            foreach (IClosed2D externalEdge_New in externalEdges_New)
            {
                Face2D face2D_New = Face2D.Create(externalEdge_New, internalEdges_New);
                if (face2D_New == null)
                {
                    continue;
                }

                List<Face2D> face2Ds_Fixed = face2D_New.FixEdges(tolerance);
                if(face2Ds_Fixed == null || face2Ds_Fixed.Count == 0)
                {
                    result.Add(face2D_New);
                }
                else
                {
                    result.AddRange(face2Ds_Fixed);
                }
            }

            return result;
        }

        public static Polygon2D Cut(this Polygon2D polygon2D, IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Tolerance.Distance)
        {
            if (polygon2D == null || segmentable2Ds == null)
                return null;

            Polygon2D result = new Polygon2D(polygon2D);

            List<Point2D> point2Ds =  polygon2D.Intersections(segmentable2Ds, tolerance);
            if (point2Ds == null || point2Ds.Count == 0)
                return result;
            
            foreach(Point2D point2D in point2Ds)
                result.InsertClosest(point2D, tolerance);

            return result;
        }
    
        public static Polyline2D Cut(this Polyline2D polyline2D, IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Tolerance.Distance)
        {
            if (polyline2D == null || segmentable2Ds == null)
                return null;

            Polyline2D result = new Polyline2D(polyline2D);

            List<Point2D> point2Ds = polyline2D.Intersections(segmentable2Ds, tolerance);
            if (point2Ds == null || point2Ds.Count == 0)
                return result;

            foreach (Point2D point2D in point2Ds)
                result.InsertClosest(point2D, tolerance);

            return result;
        }
    }
}