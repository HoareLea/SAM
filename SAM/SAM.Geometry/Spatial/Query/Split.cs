using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Split(this Face3D face3D, Shell shell, double tolerance_Snap = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(face3D == null || shell == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D_Face3D = face3D.GetBoundingBox();
            if(boundingBox3D_Face3D == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D_Shell = shell.GetBoundingBox();
            if (boundingBox3D_Shell == null)
            {
                return null;
            }

            if(!boundingBox3D_Face3D.InRange(boundingBox3D_Shell))
            {
                return null;
            }

            List<Face3D> face3Ds = shell.Face3Ds;
            if(face3Ds == null)
            {
                return null;
            }

            face3Ds.RemoveAll(x => !boundingBox3D_Face3D.InRange(x.GetBoundingBox(), tolerance_Distance));

            return face3D.Split(face3Ds, tolerance_Snap, tolerance_Angle, tolerance_Distance);
        }
    
        public static List<Face3D> Split(this Face3D face3D, IEnumerable<Shell> shells, double tolerance_Snap = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(face3D == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>() { new Face3D(face3D)};

            if(shells == null || shells.Count() == 0)
            {
                return result;
            }

            foreach(Shell shell in shells)
            {
                List<Face3D> face3Ds_Shell = new List<Face3D>();

                foreach (Face3D face3D_Temp in result)
                {
                    List<Face3D> face3Ds_Temp = Split(face3D_Temp, shell, tolerance_Snap, tolerance_Angle, tolerance_Distance);
                    if(face3Ds_Temp != null && face3Ds_Temp.Count != 0)
                    {
                        face3Ds_Shell.AddRange(face3Ds_Temp);
                    }
                    else
                    {
                        face3Ds_Shell.Add(face3D_Temp);
                    }
                }

                result = face3Ds_Shell;
            }

            return result;
        }

        public static List<Face3D> Split(this Face3D face3D, IEnumerable<Face3D> face3Ds, double tolerance_Snap = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            List<Planar.ISegmentable2D> segmentable2Ds = new List<Planar.ISegmentable2D>();
            foreach (Face3D face3D_Temp in face3Ds)
            {
                if (face3D_Temp == null)
                {
                    continue;
                }

                PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D, face3D_Temp, tolerance_Angle, tolerance_Distance);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    continue;
                }

                List<Planar.ISegmentable2D> segmentable2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<Planar.ISegmentable2D>();
                if (segmentable2Ds_Temp != null || segmentable2Ds_Temp.Count != 0)
                {
                    segmentable2Ds.AddRange(segmentable2Ds_Temp);
                }

                List<Planar.Face2D> face2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<Planar.Face2D>();
                if (face2Ds_Temp != null && face2Ds_Temp.Count != 0)
                {
                    foreach(Planar.Face2D face2D_Temp in face2Ds_Temp)
                    {
                        List<Planar.IClosed2D> edge2Ds = face2D_Temp?.Edge2Ds;
                        if(edge2Ds == null)
                        {
                            continue;
                        }

                        foreach (Planar.IClosed2D edge2D in edge2Ds)
                        {
                            if(edge2D == null)
                            {
                                continue;
                            }

                            Planar.ISegmentable2D segmentable2D = edge2D as Planar.ISegmentable2D;
                            if(segmentable2D == null)
                            {
                                throw new NotImplementedException();
                            }

                            segmentable2Ds.Add(segmentable2D);
                        }
                    }
                }
            }

            if (segmentable2Ds == null || segmentable2Ds.Count == 0)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();

            Planar.Face2D face2D = plane.Convert(face3D);

            List<Planar.Face2D> face2Ds = Planar.Query.Split(face2D, segmentable2Ds, tolerance_Snap, tolerance_Distance);
            if(face2Ds != null && face2Ds.Count > 0)
            {
                List<Planar.Face2D> face2Ds_Difference = Planar.Query.Difference(face2D, face2Ds, tolerance_Distance);
                if(face2Ds_Difference != null)
                {
                    foreach(Planar.Face2D face2D_Difference in face2Ds_Difference)
                    {
                        if(face2D_Difference != null && Planar.Query.IsValid(face2D_Difference) && face2D_Difference.GetArea() > tolerance_Snap)
                        {
                            face2Ds.Add(face2D_Difference);
                        }
                    }
                }
            }
            
            return face2Ds?.ConvertAll(x => plane.Convert(x));
        }

        public static List<Face3D> Split(this Face3D face3D_ToBeSplit, Face3D face3D, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            return face3D_ToBeSplit?.Split(new Face3D[] { face3D }, tolerance_Angle, tolerance_Distance);
        }

        public static List<Shell> Split(this IEnumerable<Shell> shells, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (shells == null)
            {
                return null;
            }

            List<Shell> result = new List<Shell>();

            int count = shells.Count();

            if (count < 2)
            {
                foreach (Shell shell in shells)
                {
                    result.Add(new Shell(shell));
                }

                return result;
            }

            foreach(Shell shell in shells)
            {
                if(shell != null)
                {
                    result.Add(shell);
                }
            }

            for (int i = 0; i < count - 1; i++)
            {
                BoundingBox3D boundingBox3D_1 = result[i].GetBoundingBox();
                if(boundingBox3D_1 == null)
                {
                    continue;
                }

                for (int j = i + 1; j < count; j++)
                {
                    BoundingBox3D boundingBox3D_2 = result[j].GetBoundingBox();
                    if (boundingBox3D_2 == null)
                    {
                        continue;
                    }

                    if(!boundingBox3D_1.InRange(boundingBox3D_2, tolerance_Distance))
                    {
                        continue;
                    }

                    List<Shell> shells_Intersection = result[i].Intersection(result[j], silverSpacing, tolerance_Angle, tolerance_Distance);
                    if(shells_Intersection == null || shells_Intersection.Count == 0)
                    {
                        continue;
                    }

                    shells_Intersection.RemoveAll(x => x == null || x.Volume(silverSpacing, tolerance_Distance) < silverSpacing || !x.IsClosed(silverSpacing));
                    if(shells_Intersection.Count == 0)
                    {
                        continue;
                    }

                    List<Shell> shells_Difference = new List<Shell>() { result[i], result[j] };
                    foreach(Shell shell_Intersection in shells_Intersection)
                    {
                        List<Shell> shells_Difference_Temp = new List<Shell>();
                        foreach (Shell shell_Difference in shells_Difference)
                        {
                            List<Shell> shells_Difference_Temp_Temp = shell_Difference.Difference(shell_Intersection, silverSpacing, tolerance_Angle, tolerance_Distance);
                            if(shells_Difference_Temp_Temp == null || shells_Difference_Temp_Temp.Count == 0)
                            {
                                continue;
                            }

                            shells_Difference_Temp.AddRange(shells_Difference_Temp_Temp);
                        }

                        shells_Difference = shells_Difference_Temp;
                    }

                    result.RemoveAt(j);
                    result.RemoveAt(i);

                    result.AddRange(shells_Intersection);
                    result.AddRange(shells_Difference);

                    return Split(result, silverSpacing, tolerance_Angle, tolerance_Distance);
                }
            }

            return result;
        }

        public static List<Segment3D> Split(this IEnumerable<Segment3D> segment3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segment3Ds == null)
                return null;

            List<Tuple<BoundingBox3D, Segment3D>> tuples = new List<Tuple<BoundingBox3D, Segment3D>>();
            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Segment3D segment3D in segment3Ds)
            {
                if (segment3D == null || segment3D.GetLength() < tolerance)
                {
                    continue;
                }

                tuples.Add(new Tuple<BoundingBox3D, Segment3D>(segment3D.GetBoundingBox(), segment3D));
                Modify.Add(point3Ds, segment3D[0], tolerance);
                Modify.Add(point3Ds, segment3D[1], tolerance);
            }

            int count = tuples.Count();

            List<List<Point3D>> point3DsList = Enumerable.Repeat<List<Point3D>>(null, count).ToList();
            for (int i = 0; i < count - 1; i++)
            {
                BoundingBox3D boundingBox3D_1 = tuples[i].Item1;
                Segment3D segment3D_1 = tuples[i].Item2;

                for (int j = i + 1; j < count; j++)
                {
                    BoundingBox3D boundingBox3D_2 = tuples[j].Item1;
                    if (!boundingBox3D_1.InRange(boundingBox3D_2, tolerance))
                    {
                        continue;
                    }

                    Segment3D segment3D_2 = tuples[j].Item2;
                    if (segment3D_1.AlmostSimilar(segment3D_2, tolerance))
                    {
                        continue;
                    }

                    Point3D point3D_Closest1;
                    Point3D point3D_Closest2;

                    List<Point3D> point3Ds_Intersection = new List<Point3D>();

                    if (segment3D_1.On(segment3D_2[0], tolerance))
                        point3Ds_Intersection.Add(segment3D_2[0]);

                    if (segment3D_2.On(segment3D_1[0], tolerance))
                        point3Ds_Intersection.Add(segment3D_1[0]);

                    if (segment3D_1.On(segment3D_2[1], tolerance))
                        point3Ds_Intersection.Add(segment3D_2[1]);

                    if (segment3D_2.On(segment3D_1[1], tolerance))
                        point3Ds_Intersection.Add(segment3D_1[1]);

                    if (point3Ds_Intersection.Count == 0)
                    {
                        Point3D point3D_Intersection = segment3D_1.Intersection(segment3D_2, out point3D_Closest1, out point3D_Closest2, tolerance);
                        if (point3D_Intersection == null || point3D_Intersection.IsNaN())
                            continue;

                        if (point3D_Closest1 != null && point3D_Closest2 != null)
                            if (point3D_Closest1.Distance(point3D_Closest2) > tolerance)
                                continue;

                        point3Ds_Intersection.Add(point3D_Intersection);
                    }

                    if (point3Ds_Intersection == null || point3Ds_Intersection.Count == 0)
                    {
                        continue;
                    }

                    foreach (Point3D point3D_Intersection in point3Ds_Intersection)
                    {
                        Point3D point3D_Intersection_Temp = point3Ds.Find(x => point3D_Intersection.AlmostEquals(x, tolerance));
                        if (point3D_Intersection_Temp == null)
                        {
                            point3D_Intersection_Temp = point3D_Intersection;
                            Modify.Add(point3Ds, point3D_Intersection_Temp, tolerance);
                        }

                        if (point3D_Intersection_Temp.Distance(segment3D_1.GetStart()) > tolerance && point3D_Intersection_Temp.Distance(segment3D_1.GetEnd()) > tolerance)
                        {
                            if (point3DsList[i] == null)
                            {
                                point3DsList[i] = new List<Point3D>();
                            }

                            Modify.Add(point3DsList[i], point3D_Intersection_Temp, tolerance);
                        }

                        if (point3D_Intersection_Temp.Distance(segment3D_2.GetStart()) > tolerance && point3D_Intersection_Temp.Distance(segment3D_2.GetEnd()) > tolerance)
                        {
                            if (point3DsList[j] == null)
                            {
                                point3DsList[j] = new List<Point3D>();
                            }

                            Modify.Add(point3DsList[j], point3D_Intersection_Temp, tolerance);
                        }
                    }
                }
            }

            List<Segment3D> result = new List<Segment3D>();
            for (int i = 0; i < count; i++)
            {
                Segment3D segment3D_Temp = tuples[i].Item2;
                if (result.Find(x => x.AlmostSimilar(segment3D_Temp, tolerance)) != null)
                    continue;

                List<Point3D> point3Ds_Temp = point3DsList[i];
                if (point3Ds_Temp == null || point3Ds_Temp.Count == 0)
                {
                    result.Add(segment3D_Temp);
                    continue;
                }

                Modify.Add(point3Ds_Temp, segment3D_Temp[0], tolerance);
                Modify.Add(point3Ds_Temp, segment3D_Temp[1], tolerance);

                Modify.SortByDistance(point3Ds_Temp, segment3D_Temp[0]);

                for (int j = 0; j < point3Ds_Temp.Count - 1; j++)
                {
                    Point3D point3D_1 = point3Ds_Temp[j];
                    Point3D point3D_2 = point3Ds_Temp[j + 1];

                    Segment3D segment3D = result.Find(x => (x[0].AlmostEquals(point3D_1, tolerance) && x[1].AlmostEquals(point3D_2, tolerance)) || (x[1].AlmostEquals(point3D_1, tolerance) && x[0].AlmostEquals(point3D_2, tolerance)));
                    if (segment3D != null)
                        continue;

                    result.Add(new Segment3D(point3D_1, point3D_2));
                }
            }

            return result;
        }

        public static Polyline3D Split(this Segment3D segment3D, double distance, AlignmentPoint alignmentPoint = AlignmentPoint.Start, double tolerance = Core.Tolerance.Distance)
        {
            if(segment3D == null || double.IsNaN(distance) || alignmentPoint == AlignmentPoint.Undefined || distance <= tolerance)
            {
                return null;
            }

            double length = segment3D.GetLength();
            if(length < distance)
            {
                return new Polyline3D(segment3D.GetPoints(), false);
            }

            Point3D point3D_Start = alignmentPoint == AlignmentPoint.End ? segment3D.GetEnd() : segment3D.GetStart();

            Vector3D vector3D = segment3D.Direction;
            Vector3D vector3D_Temp = null;

            List <Point3D> point3Ds = new List<Point3D>();

            if (alignmentPoint == AlignmentPoint.End)
            {
                vector3D.Negate();
            }
            else if(alignmentPoint == AlignmentPoint.Mid)
            {
                vector3D_Temp = vector3D * ((length % distance) / 2);
                if(vector3D_Temp.Length > tolerance)
                {
                    point3D_Start = point3D_Start.GetMoved(vector3D_Temp) as Point3D;
                    point3Ds.Add(point3D_Start);
                }
            }

            vector3D = vector3D * distance;

            int count = System.Convert.ToInt32(System.Math.Floor(length / distance));
            vector3D_Temp = new Vector3D(vector3D);

            for (int i = 0; i < count; i++)
            {
                point3Ds.Add(point3D_Start.GetMoved(vector3D_Temp) as Point3D);
                vector3D_Temp += vector3D;
            }

            if(point3Ds.Count == 0)
            {
                return null;
            }

            if (alignmentPoint == AlignmentPoint.End)
            {
                point3Ds.Reverse();
            }

            if (!AlmostSimilar( point3Ds.First(), segment3D.GetStart(), tolerance))
            {
                point3Ds.Insert(0, segment3D.GetStart());
            }

            if (!AlmostSimilar(point3Ds.Last(), segment3D.GetEnd(), tolerance))
            {
                point3Ds.Add(segment3D.GetEnd());
            }

            return new Polyline3D(point3Ds, false);


        }
    }
}