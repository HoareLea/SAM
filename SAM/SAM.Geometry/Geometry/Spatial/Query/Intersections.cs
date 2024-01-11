using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<T> Intersections<T>(this IEnumerable<Face3D> face3Ds, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where T : ISAMGeometry3D
        {
            if (face3Ds == null)
                return null;

            int count = face3Ds.Count();

            List<T> result = new List<T>();
            if (count < 2)
                return result;

            for(int i =0; i < count - 1; i++)
            {
                Face3D face3D_1 = face3Ds.ElementAt(i);
                if (face3D_1 == null)
                    continue;

                for (int j = i + 1; j < count; j++)
                {
                    Face3D face3D_2 = face3Ds.ElementAt(j);
                    if (face3D_2 == null)
                        continue;

                    PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D_1, face3D_2, tolerance_Angle, tolerance_Distance);
                    if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                        continue;

                    List<T> geometry3Ds = planarIntersectionResult.GetGeometry3Ds<T>();
                    if (geometry3Ds != null && geometry3Ds.Count > 0)
                        result.AddRange(geometry3Ds);
                }
            }

            return result;
        }
    
        public static List<Point3D> Intersections(this ISegmentable3D segmentable3D_1, ISegmentable3D segmentable3D_2, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment3D> segment3Ds_1 = segmentable3D_1?.GetSegments();
            if(segment3Ds_1 == null)
            {
                return null;
            }

            List<Segment3D> segment3Ds_2 = segmentable3D_2?.GetSegments();
            if (segment3Ds_2 == null)
            {
                return null;
            }

            List<Point3D> result = new List<Point3D>();
            foreach(Segment3D segment3D_1 in segment3Ds_1)
            {
                foreach(Segment3D segment3D_2 in segment3Ds_2)
                {
                    Point3D point3D = segment3D_1.Intersection(segment3D_2, true, tolerance);
                    if(point3D == null)
                    {
                        continue;
                    }

                    result.Add(point3D, tolerance);
                }
            }

            return result;
        }
    
        public static List<Point3D> Intersections(this ISegmentable3D segmentable3D, IEnumerable<ISegmentable3D> segmentable3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if(segmentable3D == null || segmentable3Ds == null)
            {
                return null;
            }

            List<Point3D> result = new List<Point3D>();
            foreach(ISegmentable3D segmentable3D_Temp in segmentable3Ds)
            {
                List<Point3D> point3Ds_Temp = segmentable3D.Intersections(segmentable3D_Temp);
                if(point3Ds_Temp == null || point3Ds_Temp.Count == 0)
                {
                    continue;
                }

                result.AddRange(point3Ds_Temp);
            }

            return result;
        }

        public static List<Point3D> Intersections(this Point3D point3D, Vector3D vector3D, IEnumerable<Face3D> face3Ds, bool keepDirection, bool sort = true, double tolerance = Core.Tolerance.Distance)
        {
            if(point3D == null || vector3D == null || face3Ds == null)
            {
                return null;
            }

            return IntersectionDictionary(point3D, vector3D, face3Ds, keepDirection, sort, tolerance)?.Values?.ToList();
        }

        public static List<Point3D> Intersections(this Shell shell, ISegmentable3D segmentable3D, double tolerance = Core.Tolerance.Distance)
        {
            if(shell == null || segmentable3D == null)
            {
                return null;
            }

            List<Face3D> face3Ds = shell.Face3Ds;
            if(face3Ds == null)
            {
                return null;
            }

            List<Point3D> result = new List<Point3D>();
            if(!shell.GetBoundingBox().InRange(segmentable3D.GetBoundingBox(), tolerance))
            {
                return result;
            }

            foreach(Face3D face3D in face3Ds)
            {
                PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D, segmentable3D, tolerance);
                if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    continue;
                }

                List<Point3D> point3Ds_Face3D = planarIntersectionResult.GetGeometry3Ds<Point3D>();
                if(point3Ds_Face3D == null || point3Ds_Face3D.Count == 0)
                {
                    continue;
                }

                result.AddRange(point3Ds_Face3D);
            }

            return result;
        }
    }
}