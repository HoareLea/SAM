using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static bool Snap(this List<Segment2D> segment2Ds, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (segment2Ds == null || segment2Ds.Count < 2)
                return false;

            HashSet<Point2D> point2Ds_Unique = new HashSet<Point2D>();
            for (int i = 0; i < segment2Ds.Count; i++)
            {
                List<Point2D> point2Ds_Segment2D = segment2Ds[i].GetPoints();
                foreach(Point2D point2D_Segment2D in point2Ds_Segment2D)
                {
                    if (point2Ds_Unique.Contains(point2D_Segment2D))
                        continue;

                    point2Ds_Unique.Add(point2D_Segment2D);

                    Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
                    List<Point2D> point2Ds = new List<Point2D>();
                    for (int j = 0; j < segment2Ds.Count; j++)
                    {
                        List<int> indexes = new List<int>();

                        if (segment2Ds[j][0].Distance(point2D_Segment2D) <= tolerance)
                        {
                            indexes.Add(0);
                            point2Ds.Add(segment2Ds[j][0]);
                        }

                        if (segment2Ds[j][1].Distance(point2D_Segment2D) <= tolerance)
                        {
                            indexes.Add(1);
                            point2Ds.Add(segment2Ds[j][1]);
                        }

                        if (indexes.Count == 0)
                            continue;

                        dictionary[j] = indexes;

                    }

                    Point2D point2D_Centroid = Query.Centroid(point2Ds);

                    foreach(KeyValuePair<int, List<int>> keyValuePair in dictionary)
                    {
                        if (keyValuePair.Value.Contains(0))
                            segment2Ds[keyValuePair.Key] = new Segment2D(point2D_Centroid, segment2Ds[keyValuePair.Key][1]);

                        if (keyValuePair.Value.Contains(1))
                            segment2Ds[keyValuePair.Key] = new Segment2D(segment2Ds[keyValuePair.Key][0], point2D_Centroid);
                    }
                }
            }

            segment2Ds.RemoveAll(x => x.GetLength() <= tolerance);
            return true;
        }
    }
}