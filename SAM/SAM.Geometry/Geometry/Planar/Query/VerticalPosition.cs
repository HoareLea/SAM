using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {

        public static VerticalPosition VerticalPosition(this ISegmentable2D segmentable2D, Point2D point2D, double tolerance = SAM.Core.Tolerance.Distance)
        {
            if(segmentable2D == null || point2D == null)
            {
                return Geometry.VerticalPosition.Undefined;
            }

            List<Segment2D> segment2Ds = segmentable2D.GetSegments();
            if(segment2Ds == null || segment2Ds.Count == 0)
            {
                return Geometry.VerticalPosition.Undefined;
            }

            List<VerticalPosition> verticalPositions = new List<VerticalPosition>();
            foreach(Segment2D segment2D in segmentable2D.GetSegments())
            {
                verticalPositions.Add(VerticalPosition(segment2D, point2D, tolerance));
            }

            if(verticalPositions.TrueForAll(x => x == Geometry.VerticalPosition.Undefined))
            {
                return Geometry.VerticalPosition.Undefined;
            }

            verticalPositions.RemoveAll(x => x == Geometry.VerticalPosition.Undefined);

            if (verticalPositions.TrueForAll(x => x == Geometry.VerticalPosition.Below))
            {
                return Geometry.VerticalPosition.Below;
            }

            if (verticalPositions.TrueForAll(x => x == Geometry.VerticalPosition.Above))
            {
                return Geometry.VerticalPosition.Above;
            }

            return Geometry.VerticalPosition.On;
        }

        public static VerticalPosition VerticalPosition(this Segment2D segment2D, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if(segment2D == null || point2D == null)
            {
                return Geometry.VerticalPosition.Undefined;
            }

            BoundingBox2D boundingBox = segment2D.GetBoundingBox();

            if(point2D.X < boundingBox.Min.X - tolerance || point2D.X > boundingBox.Max.X + tolerance)
            {
                return Geometry.VerticalPosition.Undefined;
            }

            Vector2D vector2D = TraceFirst(point2D, new Vector2D(0, -1), segment2D);
            if(vector2D == null)
            {
                return Geometry.VerticalPosition.Below;
            }

            if (vector2D.Length < tolerance)
            {
                return Geometry.VerticalPosition.On;
            }

            return Geometry.VerticalPosition.Above;
        }
    }
}