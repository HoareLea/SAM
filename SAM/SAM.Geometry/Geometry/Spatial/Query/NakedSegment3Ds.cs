using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Segment3D> NakedSegment3Ds(this Shell shell, int maxCount = int.MaxValue, double tolerance = Core.Tolerance.Distance)
        {
            if (shell == null)
            {
                return null;
            }

            Shell shell_Temp = new Shell(shell);
            shell_Temp.SplitEdges(tolerance);

            List<Face3D> face3Ds = shell_Temp.Face3Ds;
            if (face3Ds == null)
            {
                return null;
            }

            double tolerance_Round = tolerance * 0.1;

            List<Tuple<BoundingBox3D, Segment3D>> tuples = new List<Tuple<BoundingBox3D, Segment3D>>();
            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Face3D face3D in face3Ds)
            {
                ISegmentable3D segmentable3D = face3D?.GetExternalEdge3D() as ISegmentable3D;
                if (segmentable3D == null)
                    continue;

                List<Segment3D> segment3Ds = segmentable3D.GetSegments();
                if (segment3Ds == null)
                    continue;

                foreach (Segment3D segment3D in segment3Ds)
                {
                    if (segment3D == null || segment3D.GetLength() < tolerance)
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<BoundingBox3D, Segment3D>(segment3D.GetBoundingBox(tolerance), segment3D));
                    point3Ds.Add(segment3D.Mid());
                }

                //New Code Added (2021.07.30) -> Include Holes Edges
                List<ISegmentable3D> segmentable3Ds_Internal = face3D.GetInternalEdge3Ds()?.FindAll(x => x is ISegmentable3D).ConvertAll(x => (ISegmentable3D)x);
                if (segmentable3Ds_Internal != null)
                {
                    foreach (ISegmentable3D segmentable3D_Internal in segmentable3Ds_Internal)
                    {
                        segmentable3D_Internal?.GetSegments()?.FindAll(x => x.GetLength() > tolerance).ForEach(x => tuples.Add(new Tuple<BoundingBox3D, Segment3D>(x.GetBoundingBox(tolerance), x)));
                    }
                }
            }

            List<Segment3D> result = new List<Segment3D>();
            if (point3Ds == null || point3Ds.Count == 0)
            {
                return result;
            }

            Point3DCluster point3DCluster = new Point3DCluster(point3Ds, tolerance);
            List<List<Point3D>> point3DsList = point3DCluster.Combine();
            if(point3DsList != null && point3DsList.Count != 0)
            {
                point3Ds = new List<Point3D>();

                foreach (List<Point3D> point3Ds_Temp in point3DsList)
                {
                    point3Ds.Add(point3Ds_Temp.Average());
                }
            }
            
            foreach (Point3D point3D in point3Ds)
            {
                List<Tuple<BoundingBox3D, Segment3D>> tuples_Temp = tuples.FindAll(x => x.Item1.Inside(point3D, true, tolerance));
                if (tuples_Temp == null || tuples_Temp.Count == 0)
                    continue;

                if (tuples_Temp.Count == 1)
                {
                    if (Core.Query.Round(tuples_Temp[0].Item2.Distance(point3D), tolerance) <= tolerance)
                        result.Add(tuples_Temp[0].Item2);

                    if (result.Count >= maxCount)
                        return result;

                    continue;
                }

                //tuples_Temp = tuples_Temp.FindAll(x => x.Item2.On(point3D, tolerance));

                //Removed (2022.09.01)
                //tuples_Temp = tuples_Temp.FindAll(x => Core.Query.Round(x.Item2.Distance(point3D), tolerance) > tolerance);

                tuples_Temp.RemoveAll(x => Core.Query.Round(x.Item2.Distance(point3D), tolerance) > tolerance);
                if (tuples_Temp == null || tuples_Temp.Count == 0)
                {
                    continue;
                }

                if (tuples_Temp.Count == 1)
                {
                    result.Add(tuples_Temp[0].Item2);

                    if (result.Count >= maxCount)
                    {
                        return result;
                    }

                    continue;
                }
            }

            return result;
        }

        //Removed 2025.03.20
        //public static List<Segment3D> NakedSegment3Ds(this Shell shell, int maxCount = int.MaxValue, double tolerance = Core.Tolerance.Distance)
        //{
        //    if(shell == null)
        //    {
        //        return null;
        //    }

        //    Shell shell_Temp = new Shell(shell);
        //    shell_Temp.SplitEdges(tolerance);

        //    List<Face3D> face3Ds = shell_Temp.Face3Ds;
        //    if (face3Ds == null)
        //    {
        //        return null;
        //    }

        //    List<Tuple<BoundingBox3D, Segment3D>> tuples = new List<Tuple<BoundingBox3D, Segment3D>>();
        //    List<Point3D> point3Ds = new List<Point3D>();
        //    foreach (Face3D face3D in face3Ds)
        //    {
        //        ISegmentable3D segmentable3D = face3D?.GetExternalEdge3D() as ISegmentable3D;
        //        if (segmentable3D == null)
        //            continue;

        //        List<Segment3D> segment3Ds = segmentable3D.GetSegments();
        //        if (segment3Ds == null)
        //            continue;

        //        foreach (Segment3D segment3D in segment3Ds)
        //        {
        //            if (segment3D == null || segment3D.GetLength() < tolerance)
        //                continue;

        //            tuples.Add(new Tuple<BoundingBox3D, Segment3D>(segment3D.GetBoundingBox(tolerance), segment3D));
        //            Modify.Add(point3Ds, segment3D.Mid(), tolerance);
        //        }

        //        //New Code Added (2021.07.30) -> Include Holes Edges
        //        List<ISegmentable3D> segmentable3Ds_Internal = face3D.GetInternalEdge3Ds()?.FindAll(x => x is ISegmentable3D).ConvertAll(x => (ISegmentable3D)x);
        //        if(segmentable3Ds_Internal != null)
        //        {
        //            foreach(ISegmentable3D segmentable3D_Internal in segmentable3Ds_Internal)
        //            {
        //                segmentable3D_Internal?.GetSegments()?.FindAll(x => x.GetLength() > tolerance).ForEach(x => tuples.Add(new Tuple<BoundingBox3D, Segment3D>(x.GetBoundingBox(tolerance), x)));
        //            }
        //        }
        //    }

        //    List<Segment3D> result = new List<Segment3D>();
        //    if (point3Ds == null || point3Ds.Count == 0)
        //            return result;

        //    foreach (Point3D point3D in point3Ds)
        //    {
        //        List<Tuple<BoundingBox3D, Segment3D>> tuples_Temp = tuples.FindAll(x => x.Item1.Inside(point3D, true, tolerance));
        //        if (tuples_Temp == null || tuples_Temp.Count == 0)
        //            continue;

        //        if (tuples_Temp.Count == 1)
        //        {
        //            if (Core.Query.Round(tuples_Temp[0].Item2.Distance(point3D), tolerance) <= tolerance)
        //                result.Add(tuples_Temp[0].Item2);

        //            if (result.Count >= maxCount)
        //                return result;

        //            continue;
        //        }

        //        tuples_Temp = tuples_Temp.FindAll(x => x.Item2.On(point3D, tolerance));

        //        //Removed (2022.09.01)
        //        //tuples_Temp = tuples_Temp.FindAll(x => Core.Query.Round(x.Item2.Distance(point3D), tolerance) > tolerance);

        //        tuples_Temp.RemoveAll(x => Core.Query.Round(x.Item2.Distance(point3D), tolerance) > tolerance);
        //        if (tuples_Temp == null || tuples_Temp.Count == 0)
        //            continue;

        //        if (tuples_Temp.Count == 1)
        //        {
        //            result.Add(tuples_Temp[0].Item2);

        //            if (result.Count >= maxCount)
        //                return result;

        //            continue;
        //        }
        //    }

        //    return result;
        //}
    }
}