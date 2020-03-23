﻿using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Vector2D> Trace(this Point2D point2D, Vector2D vector2D, IEnumerable<ISegmentable2D> segmentable2Ds, int bounces = 1)
        {
            if (point2D == null || vector2D == null || segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            HashSet<Point2D> point2Ds = new HashSet<Point2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                if (segment2Ds_Temp == null)
                    continue;

                foreach (Segment2D segment2D in segment2Ds_Temp)
                {
                    if (segment2D == null)
                        continue;

                    segment2Ds.Add(segment2D);
                    point2Ds.Add(segment2D.GetStart());
                    point2Ds.Add(segment2D.GetEnd());
                }

                segment2Ds.AddRange(segment2Ds);
            }

            BoundingBox2D boundingBox2D = new BoundingBox2D(point2Ds, 1);
            double length = new Vector2D(boundingBox2D.Min, boundingBox2D.Max).Length;

            List<Vector2D> result = new List<Vector2D>();
            Dictionary<Point2D, Segment2D> dictionary_Intersections = null;
            Vector2D vector2D_Temp = vector2D.Unit * length;
            Point2D point2D_Temp = point2D;

            do
            {
                dictionary_Intersections = Query.IntersectionDictionary(point2D_Temp, vector2D_Temp, segment2Ds, true, true);
                if(dictionary_Intersections != null && dictionary_Intersections.Count > 0)
                {
                    List<Point2D> point2Ds_Intersections = dictionary_Intersections.Keys.ToList();
                    Modify.SortByDistance(point2Ds_Intersections, point2D_Temp);

                    Vector2D vector2D_result = new Vector2D(point2D_Temp, point2Ds_Intersections[0]);
                    result.Add(vector2D_result);

                    point2D_Temp = dictionary_Intersections.Keys.First();

                    vector2D_Temp = Query.Bounce(vector2D_Temp, dictionary_Intersections[point2D_Temp]);
                }
            }
            while (result.Count <= bounces && dictionary_Intersections != null && dictionary_Intersections.Count > 0);

            return result;
        }
    }
}
