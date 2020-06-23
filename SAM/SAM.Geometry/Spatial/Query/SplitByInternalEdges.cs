using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> SplitByInternalEdges(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null)
                return null;

            List<IClosed2D> internalEdges = face3D.InternalEdges;
            if (internalEdges == null || internalEdges.Count == 0)
                return new List<Face3D>() { (Face3D)face3D.Clone() };

            ISegmentable2D externalEdge = face3D.ExternalEdge as ISegmentable2D;
            if (externalEdge == null)
                throw new NotImplementedException();

            List<Point2D> point2Ds = externalEdge.GetPoints();
            if (point2Ds == null || point2Ds.Count < 3)
                return null;

            List<Segment2D> segment2Ds = externalEdge.GetSegments();

            foreach (IClosed2D internalEdge_Closed2D in internalEdges)
            {
                ISegmentable2D internalEdge = internalEdge_Closed2D as ISegmentable2D;
                if (internalEdge == null)
                    throw new NotImplementedException();

                List<Tuple<Point2D, Point2D>> tuples = new List<Tuple<Point2D, Point2D>>();

                point2Ds.ForEach(x => tuples.Add(new Tuple<Point2D, Point2D>(Planar.Query.Closest(internalEdge, x), x)));

                if (tuples.Count < 3)
                    continue;
                
                tuples.Sort((x, y) => x.Item1.Distance(x.Item2).CompareTo(y.Item1.Distance(y.Item2)));

                segment2Ds.Add(new Segment2D(tuples[0].Item1, tuples[0].Item2));
                segment2Ds.Add(new Segment2D(tuples[1].Item1, tuples[1].Item2));
                segment2Ds.AddRange(internalEdge.GetSegments());
            }


            List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance);
            if (polygon2Ds == null)
                return null;

            polygon2Ds.RemoveAll(x => !face3D.Inside(x.InternalPoint2D()));

            Plane plane = face3D.GetPlane();

            return polygon2Ds.ConvertAll(x => new Face3D(plane, new Face2D(x)));
        }
    }
}