using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool InRange(this BoundingBox3D boundingBox3D, Point3D point3D, double tolerance = Tolerance.Distance)
        {
            if (boundingBox3D == null || point3D == null)
                return false;

            return boundingBox3D.Inside(point3D, true, tolerance);
        }

        public static bool InRange(this BoundingBox3D boundingBox3D_1, BoundingBox3D boundingBox3D_2, double tolerance = Tolerance.Distance)
        {
            //if (boundingBox3D_1 == null || boundingBox3D_2 == null)
            //    return false;

            //return boundingBox3D_1.Inside(boundingBox3D_2) || boundingBox3D_1.Intersect(boundingBox3D_2) || boundingBox3D_1.On(boundingBox3D_2.Min) || boundingBox3D_1.On(boundingBox3D_2.Max);

            if (boundingBox3D_1 == null || boundingBox3D_2 == null)
            {
                return false;
            }

            double max_1;
            double min_1;

            double max_2;
            double min_2;

            max_1 = boundingBox3D_1.Max.X + tolerance;
            min_1 = boundingBox3D_1.Min.X - tolerance;

            max_2 = boundingBox3D_2.Max.X;
            min_2 = boundingBox3D_2.Min.X;

            if (max_1 < min_2 || min_1 > max_2)
            {
                return false;
            }

            max_1 = boundingBox3D_1.Max.Y + tolerance;
            min_1 = boundingBox3D_1.Min.Y - tolerance;

            max_2 = boundingBox3D_2.Max.Y;
            min_2 = boundingBox3D_2.Min.Y;

            if (max_1 < min_2 || min_1 > max_2)
            {
                return false;
            }

            max_1 = boundingBox3D_1.Max.Z + tolerance;
            min_1 = boundingBox3D_1.Min.Z - tolerance;

            max_2 = boundingBox3D_2.Max.Z;
            min_2 = boundingBox3D_2.Min.Z;

            if (max_1 < min_2 || min_1 > max_2)
            {
                return false;
            }

            return true;
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

        public static bool InRange(this BoundingBox3D boundingBox3D, ISegmentable3D segmentable3D, double tolerance = Tolerance.Distance)
        {
            List<Segment3D> segment3Ds = segmentable3D?.GetSegments();
            if (segment3Ds == null || segment3Ds.Count == 0)
            {
                return false;
            }

            return segment3Ds.Find(x => InRange(boundingBox3D, x, tolerance)) != null;
        }

        public static bool InRange(this BoundingBox3D boundingBox3D, Face3D face3D, double tolerance = Tolerance.Distance)
        {
            if (face3D == null || boundingBox3D == null)
            {
                return false;
            }

            IClosedPlanar3D closedPlanar3D = face3D.GetExternalEdge3D();
            if(closedPlanar3D == null)
            {
                return false;
            }

            ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
            if (segmentable3D == null)
            {
                throw new NotImplementedException();
            }

            return InRange(boundingBox3D, segmentable3D, tolerance);
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
                    if (boundingBox3D_Shell.InRange(point3D, tolerance) && (shell.InRange(point3D, tolerance) || shell.Inside(point3D, tolerance)))
                    {
                        add = true;
                        break;
                    }

                    point3D = segment3D[1];
                    if (boundingBox3D_Shell.InRange(point3D, tolerance) && (shell.InRange(point3D, tolerance) || shell.Inside(point3D, tolerance)))
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

        public static List<T> InRange<T>(this Shell shell, IEnumerable<T> face3DObjects, double tolerance = Tolerance.Distance) where T :IFace3DObject
        {
            if(shell == null || face3DObjects == null)
            {
                return null;
            }

            Dictionary<Face3D, T> dictionary = new Dictionary<Face3D, T>();
            foreach(T face3DObject in face3DObjects)
            {
                Face3D face3D = face3DObject?.Face3D;
                if(face3D == null)
                {
                    continue;
                }
                dictionary[face3D] = face3DObject;
            }

            List<Face3D> face3Ds = InRange(shell, dictionary.Keys, tolerance);
            if(face3Ds == null)
            {
                return null;
            }

            return face3Ds.ConvertAll(x => dictionary[x]);

        }

        public static List<T> InRange<T>(this IEnumerable<T> face3DObjects, Point3D point3D, Range<double> range, bool sort = true, int count = int.MaxValue, double tolerance = Tolerance.Distance) where T : IFace3DObject
        {
            if(face3DObjects == null || point3D == null || range == null)
            {
                return null;
            }

            List<Tuple<double, T>> tuple = new List<Tuple<double, T>>();
            foreach(T face3DObject in face3DObjects)
            {
                Face3D face3D = face3DObject?.Face3D;

                if (face3D == null)
                {
                    continue;
                }

                double distance = face3D.Distance(point3D, tolerance);

                if(!range.In(distance))
                {
                    continue;
                }

                tuple.Add(new Tuple<double, T>(distance, face3DObject));

                if(tuple.Count >= count)
                {
                    return tuple.ConvertAll(x => x.Item2);
                }
            }

            return tuple.ConvertAll(x => x.Item2);
        }
    
        public static bool InRange(this BoundingBox3D boundingBox3D, ISAMGeometry3DObject sAMGeometry3DObject, double tolerance = Tolerance.Distance)
        {
            if(boundingBox3D == null || sAMGeometry3DObject == null)
            {
                return false;
            }

            if (sAMGeometry3DObject is IEnumerable<ISAMGeometry3DObject>)
            {
                foreach (ISAMGeometry3DObject sAMGeometry3DObject_Temp in (IEnumerable<ISAMGeometry3DObject>)sAMGeometry3DObject)
                {
                    if (InRange(boundingBox3D, sAMGeometry3DObject_Temp, tolerance))
                    {
                        return true;
                    }
                }

                return false;
            }

            ISAMGeometry3D sAMGeometry3D = sAMGeometry3DObject.ISAMGeometry3D();
            if(sAMGeometry3D == null)
            {
                return false;
            }

            if(sAMGeometry3D is Point3D)
            {
                return boundingBox3D.InRange((Point3D)sAMGeometry3D, tolerance);
            }

            if(sAMGeometry3D is Segment3D)
            {
                return boundingBox3D.InRange((Segment3D)sAMGeometry3D, tolerance);
            }

            if (sAMGeometry3D is BoundingBox3D)
            {
                return boundingBox3D.InRange((BoundingBox3D)sAMGeometry3D, tolerance);
            }

            if(sAMGeometry3D is Shell)
            {
                return InRange((Shell)sAMGeometry3D, boundingBox3D, tolerance);
            }

            if (sAMGeometry3D is Face3D)
            {
                return InRange(boundingBox3D, (Face3D)sAMGeometry3D, tolerance);
            }

            if (sAMGeometry3D is ISegmentable3D)
            {
                return InRange(boundingBox3D, (ISegmentable3D)sAMGeometry3D, tolerance);
            }

            return false;
        }
    }
}