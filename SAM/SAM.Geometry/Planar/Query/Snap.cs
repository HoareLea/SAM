using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay.Snap;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polygon2D> Snap(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double snapDistance, double tolerance = Core.Tolerance.Distance)
        {
            LinearRing linearRing_1 = (polygon2D_1 as IClosed2D)?.ToNTS(tolerance);
            if (linearRing_1 == null)
                return null;

            LinearRing linearRing_2 = (polygon2D_2 as IClosed2D)?.ToNTS(tolerance);
            if (linearRing_2 == null)
                return null;

            NetTopologySuite.Geometries.Geometry[] geometries = GeometrySnapper.Snap(linearRing_1, linearRing_2, snapDistance);
            if (geometries == null)
                return null;

            return geometries.ToList().ConvertAll(x => (x as LinearRing).ToSAM(tolerance));
        }

        public static List<Face2D> Snap(this Face2D face2D_1, Face2D face2D_2, double snapDistance, double tolerance = Core.Tolerance.Distance)
        {
            Polygon polygon_1 = Convert.ToNTS(face2D_1 as Face, tolerance);
            if (polygon_1 == null)
                return null;

            Polygon polygon_2 = Convert.ToNTS(face2D_2 as Face, tolerance);
            if (polygon_2 == null)
                return null;

            NetTopologySuite.Geometries.Geometry[] geometries = GeometrySnapper.Snap(polygon_1, polygon_2, snapDistance);
            if (geometries == null)
                return null;

            return geometries.ToList().ConvertAll(x => (x as Polygon).ToSAM(tolerance));
        }

        public static List<ISAMGeometry2D> Snap(this Face2D face2D_1, Segment2D segment2D, double snapDistance, double tolerance = Core.Tolerance.Distance)
        {
            Polygon polygon = Convert.ToNTS(face2D_1 as Face, tolerance);
            if (polygon == null)
                return null;

            LineString lineString = segment2D?.ToNTS(tolerance);
            if (lineString == null)
                return null;

            NetTopologySuite.Geometries.Geometry[] geometries = GeometrySnapper.Snap(polygon, lineString, snapDistance);
            if (geometries == null)
                return null;

            return geometries.ToList().ConvertAll(x => x.ToSAM(tolerance));
        }
    
        public static Polygon2D Snap(this Polygon2D polygon2D, IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (polygon2D == null || segmentable2Ds == null)
                return null;

            List<Point2D> point2Ds = polygon2D?.Points;
            if (point2Ds == null || point2Ds.Count == 0)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                    continue;

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if (segment2Ds_Temp != null && segment2Ds_Temp.Count > 0)
                    segment2Ds.AddRange(segment2Ds_Temp);
            }

            for (int j = 0; j < point2Ds.Count; j++)
            {
                Point2D point2D = point2Ds[j];

                Segment2D segment2D = null;

                segment2D = segment2Ds.Find(x => x[0].AlmostEquals(point2D, tolerance));
                if (segment2D != null)
                {
                    point2Ds[j] = segment2D[0];
                    continue;
                }

                segment2D = segment2Ds.Find(x => x[1].AlmostEquals(point2D, tolerance));
                if (segment2D != null)
                {
                    point2Ds[j] = segment2D[1];
                    continue;
                }

                double distance = double.MaxValue;
                foreach (Segment2D segment2D_Temp in segment2Ds)
                {
                    Point2D point2D_Temp = segment2D_Temp.Closest(point2D);
                    double distance_Temp = point2D_Temp.Distance(point2D);
                    if (distance_Temp < distance)
                    {
                        point2Ds[j] = point2D_Temp;
                        distance = distance_Temp;
                    }

                }
            }

            return new Polygon2D(point2Ds);
        }

        public static Polygon2D Snap(this Polygon2D polygon2D, IEnumerable<Point2D> point2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (point2Ds == null)
                return null;

            List<Point2D> point2Ds_Result = polygon2D?.Points;
            if (point2Ds == null || point2Ds_Result.Count == 0)
                return null;

            for (int j = 0; j < point2Ds_Result.Count; j++)
            {
                double distance = double.MaxValue;
                foreach (Point2D point2D_Temp in point2Ds)
                {
                    if (point2D_Temp == null)
                        continue;

                    double distance_Temp = point2D_Temp.Distance(point2Ds_Result[j]);
                    if (distance_Temp > 0 && distance_Temp <= tolerance && distance > distance_Temp)
                    {
                        point2Ds_Result[j] = point2D_Temp;
                        distance = distance_Temp;
                    }
                }
            }

            return new Polygon2D(point2Ds_Result);
        }

        /// <summary>
        /// Snap points for each segment in given tolerance. Points will be snapped to average points in range of tolerance
        /// </summary>
        /// <param name="segment2Ds">List of Segments to be snapped</param>
        /// <param name="includeIntersection">Intersection point will be applied if there is only two segments to be snapped</param>
        /// <param name="tolerance">Snapping tolerance</param>
        /// <returns></returns>
        public static List<Segment2D> Snap(this IEnumerable<Segment2D> segment2Ds, bool includeIntersection = true, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (segment2Ds == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            foreach(Segment2D segment2D in segment2Ds)
            {
                if(segment2D == null)
                {
                    result.Add(null);
                    continue;
                }
                result.Add(new Segment2D(segment2D));
            }

            HashSet<Point2D> point2Ds_Unique = new HashSet<Point2D>();
            for (int i = 0; i < result.Count; i++)
            {
                List<Point2D> point2Ds_Segment2D = result[i].GetPoints();
                foreach (Point2D point2D_Segment2D in point2Ds_Segment2D)
                {
                    if (point2Ds_Unique.Contains(point2D_Segment2D))
                        continue;

                    point2Ds_Unique.Add(point2D_Segment2D);

                    Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
                    List<Point2D> point2Ds = new List<Point2D>();
                    for (int j = 0; j < result.Count; j++)
                    {
                        List<int> indexes = new List<int>();

                        if (result[j][0].Distance(point2D_Segment2D) <= tolerance)
                        {
                            indexes.Add(0);
                            point2Ds.Add(result[j][0]);
                        }

                        if (result[j][1].Distance(point2D_Segment2D) <= tolerance)
                        {
                            indexes.Add(1);
                            point2Ds.Add(result[j][1]);
                        }

                        if (indexes.Count == 0)
                            continue;

                        dictionary[j] = indexes;

                    }

                    Point2D point2D_New = null;
                    if (point2Ds.Count == 1)
                    {
                        point2D_New = point2Ds[0];
                    }
                    else if (point2Ds.Count == 2 && includeIntersection)
                    {
                        IEnumerable<int> indexes = dictionary.Keys;
                        if (indexes.Count() == 2)
                            point2D_New = result[indexes.ElementAt(0)].Intersection(result[indexes.ElementAt(1)], false, tolerance);
                    }

                    if (point2D_New == null)
                        point2D_New = Query.Average(point2Ds);


                    foreach (KeyValuePair<int, List<int>> keyValuePair in dictionary)
                    {
                        if (keyValuePair.Value.Contains(0))
                            result[keyValuePair.Key] = new Segment2D(point2D_New, result[keyValuePair.Key][1]);

                        if (keyValuePair.Value.Contains(1))
                            result[keyValuePair.Key] = new Segment2D(result[keyValuePair.Key][0], point2D_New);
                    }
                }
            }

            result.RemoveAll(x => x.GetLength() <= tolerance);
            return result;
        }
    }
}