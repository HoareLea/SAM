using SAM.Core;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool InRange(this BoundingBox3D boundingBox3D, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (boundingBox3D == null || point3D == null)
                return false;

            return boundingBox3D.Inside(point3D, true, tolerance);
        }

        public static bool InRange(this BoundingBox3D boundingBox3D_1, BoundingBox3D boundingBox3D_2, double tolerance = Tolerance.Distance)
        {
            if (boundingBox3D_1 == null || boundingBox3D_2 == null)
                return false;

            return boundingBox3D_1.Inside(boundingBox3D_2) || boundingBox3D_1.Intersect(boundingBox3D_2) || boundingBox3D_1.On(boundingBox3D_2.Min) || boundingBox3D_1.On(boundingBox3D_2.Max);
        }

        public static bool InRange(this BoundingBox3D boundingBox3D, Segment3D segment3D, double tolerance = Tolerance.Distance)
        {
            if (boundingBox3D == null || segment3D == null)
                return false;

            if (boundingBox3D.Inside(segment3D[0]) || boundingBox3D.Inside(segment3D[1]) || boundingBox3D.On(segment3D[0], tolerance) || boundingBox3D.On(segment3D[1], tolerance))
                return true;

            if (boundingBox3D.Intersect(segment3D, tolerance))
                return true;

            return false;
        }

        public static bool InRange(this Shell shell, BoundingBox3D boundingBox3D, double tolerance = Tolerance.Distance)
        {
            if (shell == null || boundingBox3D == null)
                return false;

            List<BoundingBox3D> boundingBox3Ds = InRange(shell, new List<BoundingBox3D>() { boundingBox3D }, tolerance);
            return boundingBox3Ds != null && boundingBox3Ds.Count != 0;
        }

        public static List<BoundingBox3D> InRange(this Shell shell, IEnumerable<BoundingBox3D> boundingBox3Ds, double tolerance = Tolerance.Distance)
        {
            if (shell == null || boundingBox3Ds == null)
                return null;

            BoundingBox3D boundingBox3D_Shell = shell.GetBoundingBox();

            List<BoundingBox3D> result = new List<BoundingBox3D>();

            foreach(BoundingBox3D boundingBox3D in boundingBox3Ds)
            {
                if (!boundingBox3D_Shell.InRange(boundingBox3D, tolerance))
                    continue;

                List<Segment3D> segment3Ds = boundingBox3D.GetSegments();
                if (segment3Ds == null)
                    continue;

                int count = result.Count;

                List<Segment3D> segment3Ds_Temp = new List<Segment3D>();
                foreach (Segment3D segment3D in segment3Ds)
                {
                    if (!boundingBox3D_Shell.InRange(segment3D, tolerance))
                        continue;

                    segment3Ds_Temp.Add(segment3D);

                    Point3D point3D = null;

                    point3D = segment3D[0];
                    if (boundingBox3D_Shell.InRange(point3D, tolerance) && shell.InRange(point3D, tolerance))
                    {
                        result.Add(boundingBox3D);
                        break;
                    }

                    point3D = segment3D[1];
                    if (boundingBox3D_Shell.InRange(point3D, tolerance) && shell.InRange(point3D, tolerance))
                    {
                        result.Add(boundingBox3D);
                        break;
                    }
                }

                if (count != result.Count)
                    continue;

                foreach (Segment3D segment3D in segment3Ds_Temp)
                {
                    List<Point3D> point3Ds = shell.IntersectionPoint3Ds(segment3D, false, tolerance);
                    if (point3Ds != null && point3Ds.Count != 0)
                    {
                        result.Add(boundingBox3D);
                        break;
                    }
                }
            }

            return result;
        }

        public static bool InRange(this Shell shell, Segment3D segment3D, double tolerance = Tolerance.Distance)
        {
            if (shell == null || segment3D == null)
                return false;

            List<ISegmentable3D> segmentable3Ds = InRange(shell, new List<ISegmentable3D>() { segment3D }, tolerance);
            return segmentable3Ds != null && segmentable3Ds.Count != 0;
        }

        public static List<ISegmentable3D> InRange(this Shell shell, IEnumerable<ISegmentable3D> segmentable3Ds, double tolerance = Tolerance.Distance)
        {
            if (shell == null || segmentable3Ds == null)
                return null;

            BoundingBox3D boundingBox3D_Shell = shell.GetBoundingBox();

            List<ISegmentable3D> result = new List<ISegmentable3D>();

            foreach (ISegmentable3D segmentable3D in segmentable3Ds)
            {
                if (!boundingBox3D_Shell.InRange(segmentable3D.GetBoundingBox(), tolerance))
                    continue;

                List<Segment3D> segment3Ds = segmentable3D.GetSegments();
                if (segment3Ds == null || segment3Ds.Count == 0)
                    continue;

                bool add = false;
                foreach(Segment3D segment3D in segment3Ds)
                {
                    Point3D point3D = null;

                    point3D = segment3D[0];
                    if (boundingBox3D_Shell.InRange(point3D, tolerance) && shell.InRange(point3D, tolerance))
                    {
                        add = true;
                        break;
                    }

                    point3D = segment3D[1];
                    if (boundingBox3D_Shell.InRange(point3D, tolerance) && shell.InRange(point3D, tolerance))
                    {
                        add = true;
                        break;
                    }
                }

                if(add)
                {
                    result.Add(segmentable3D);
                    continue;
                }

                foreach (Segment3D segment2D in segment3Ds)
                {
                    List<Point3D> point3Ds_Temp = shell.IntersectionPoint3Ds(segment2D, false, tolerance);
                    if (point3Ds_Temp != null && point3Ds_Temp.Count != 0)
                    {
                        add = true;
                        break;
                    }
                }

                if (add)
                {
                    result.Add(segmentable3D);
                    continue;
                }
            }

            return result;
        }

        public static List<Face3D> InRange(this Shell shell, IEnumerable<Face3D> face3Ds, double tolerance = Tolerance.Distance)
        {
            if (shell == null || face3Ds == null)
                return null;

            BoundingBox3D boundingBox3D_Shell = shell.GetBoundingBox();

            Dictionary<ISegmentable3D, Face3D> dictionary = new Dictionary<ISegmentable3D, Face3D>();
            foreach(Face3D face3D in face3Ds)
            {
                ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
                if (segmentable3D == null)
                    continue;

                dictionary[segmentable3D] = face3D;
            }

            List<ISegmentable3D> segmentable3Ds = InRange(shell, dictionary.Keys, tolerance);
            if (segmentable3Ds == null)
                return null;

            List<Face3D> result = new List<Face3D>();
            foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                result.Add(dictionary[segmentable3D]);

            return result;
        }
    }
}