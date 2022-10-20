using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Polygon2D MoveSegment2D(this Polygon2D polygon2D, int index, Vector2D vector2D, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(index < 0 || vector2D == null)
            {
                return null;
            }

            List<Segment2D> segment2Ds = polygon2D?.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
            {
                return null;
            }

            if (index >= segment2Ds.Count)
            {
                return null;
            }

            int index_Next = Core.Query.Next(segment2Ds.Count, index);
            if (index_Next == -1)
            {
                return null;
            }

            int index_Previous = Core.Query.Previous(segment2Ds.Count, index);
            if (index_Previous == -1)
            {
                return null;
            }

            Segment2D segment2D = segment2Ds[index];
            segment2D.Move(vector2D);

            Segment2D segment2D_Previous = segment2Ds[index_Previous];
            Segment2D segment2D_Next = segment2Ds[index_Next];

            if(segment2D_Next.Direction.SmallestAngle(segment2D_Previous.Direction) <= tolerance_Angle)
            {
                segment2Ds.Insert(index_Previous, new Segment2D(segment2D_Previous[1], segment2D[0]));
                segment2Ds.Insert(index_Next, new Segment2D(segment2D[1], segment2D_Next[0]));
            }
            else
            {
                Point2D point2D_Intersection = null;

                point2D_Intersection = segment2D.Intersection(segment2D_Previous, false, tolerance_Distance);
                if(point2D_Intersection != null)
                {
                    segment2D_Previous = new Segment2D(segment2D_Previous[0], point2D_Intersection);
                    segment2Ds[index_Previous] = segment2D_Previous;

                    segment2D = new Segment2D(point2D_Intersection, segment2D[1]);
                    segment2Ds[index] = segment2D;
                }

                point2D_Intersection = segment2D.Intersection(segment2D_Next, false, tolerance_Distance);
                if (point2D_Intersection != null)
                {
                    segment2D_Next= new Segment2D(point2D_Intersection, segment2D_Next[1]);
                    segment2Ds[index_Next] = segment2D_Next;

                    segment2D = new Segment2D(segment2D[0], point2D_Intersection);
                    segment2Ds[index] = segment2D;
                }
            }

            List<Point2D> point2Ds = new List<Point2D>();
            foreach(Segment2D segment2D_Temp in segment2Ds)
            {
                point2Ds.Add(segment2D_Temp[0]);
            }
            point2Ds.Add(segment2Ds.Last()[1]);
            
            return new Polygon2D(point2Ds);
        }
    }
}