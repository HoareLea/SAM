using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Cut(this IEnumerable<Face3D> face3Ds, double elevation, double tolerance = Tolerance.Distance)
        {
            if (face3Ds == null || double.IsNaN(elevation))
                return null;

            Plane plane = Plane.WorldXY;
            plane.GetMoved(new Vector3D(0, 0, elevation));

            List<Face3D> result = new List<Face3D>();
            foreach(Face3D face3D in face3Ds)
            {
                BoundingBox3D boundingBox3D = face3D?.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                if(!(boundingBox3D.Max.Z > elevation && boundingBox3D.Min.Z < elevation))
                {
                    result.Add(face3D);
                    continue;
                }

                List<Face3D> face3Ds_Cut = face3D.Cut(plane, tolerance);
                if(face3Ds_Cut == null || face3Ds_Cut.Count == 0 || face3Ds_Cut.Count == 1)
                {
                    result.Add(face3D);
                    continue;
                }

                result.AddRange(face3Ds_Cut);
            }

            return result;
        }

        public static List<Face3D> Cut(this Face3D face3D, Plane plane, double tolerance = Tolerance.Distance)
        {
            PlanarIntersectionResult planarIntersectionResult = plane?.PlanarIntersectionResult(face3D, tolerance);
            if (planarIntersectionResult == null)
                return null;

            if (!planarIntersectionResult.Intersecting)
                return null;

            List<Segment3D> segment3Ds = planarIntersectionResult.GetGeometry3Ds<Segment3D>();
            if (segment3Ds == null || segment3Ds.Count == 0)
                return null;

            Plane plane_Face3D = face3D.GetPlane();
            Face2D face2D = plane_Face3D.Convert(face3D);
            if (face2D == null)
                return null;

            List<Segment2D> segment2Ds = segment3Ds.ConvertAll(x => plane_Face3D.Convert(plane_Face3D.Project(x)));
            if (segment2Ds == null)
                return null;

            List<Face2D> face2Ds = face2D.Cut(segment2Ds, tolerance);
            if (face2Ds == null || face2Ds.Count == 0)
                return null;

            List<Face3D> result = face2Ds.ConvertAll(x => plane_Face3D.Convert(x));

            //for(int i=result.Count - 1; i >= 0; i--)
            //{
            //    if(result[i] == null)
            //    {
            //        result.RemoveAt(i);
            //        continue;
            //    }

            //    Face3D face3D_Snap = result[i].Snap(face3D, tolerance, tolerance);
            //    if(face3D_Snap != null)
            //    {
            //        result[i] = face3D_Snap;
            //    }
            //}

            return result;
        }

        public static List<Face3D> Cut(this Face3D face3D, Plane plane, out List<Face3D> face3Ds_Above, out List<Face3D> face3Ds_Below, double tolerance = Tolerance.Distance)
        {
            face3Ds_Above = null;
            face3Ds_Below = null;

            List<Face3D> result = Cut(face3D, plane, tolerance);
            if (result == null)
                return null;

            face3Ds_Above = new List<Face3D>();
            face3Ds_Below = new List<Face3D>();

            if (result.Count == 0)
                return result;

            foreach(Face3D face3D_New in result)
            {
                Point3D point3D = face3D_New?.InternalPoint3D();
                if (point3D == null)
                    continue;

                if (plane.Above(point3D, tolerance))
                    face3Ds_Above.Add(face3D_New);
                else
                    face3Ds_Below.Add(face3D_New);
            }

            return result;
        }

        /// <summary>
        /// Cut Face3D by given plane. Returned face3Ds will be limited to the ones on the same side as given Point3D.
        /// </summary>
        /// <param name="face3D">Face3D to be cut</param>
        /// <param name="plane">Cutting plane</param>
        /// <param name="point3D">Point3D which determines faces to be left</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Face3Ds </returns>
        public static List<Face3D> Cut(this Face3D face3D, Plane plane, Point3D point3D, double tolerance = Tolerance.Distance)
        {
            if (point3D == null || face3D == null || plane == null)
                return null;

            Cut(face3D, plane, out List<Face3D> face3Ds_Above, out List<Face3D> face3Ds_Below, tolerance);

            if (plane.Above(point3D, tolerance))
                return face3Ds_Above;

            return face3Ds_Below;
        }

        public static List<Face3D> Cut(this Face3D face3D, IEnumerable<Face3D> face3Ds, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance)
        {
            if (face3D == null || face3Ds == null)
                return null;

            Plane plane = face3D.GetPlane();
            if (plane == null)
                return null;

            List<ISegmentable2D> segmentable2Ds = new List<ISegmentable2D>();
            foreach(Face3D face3D_Temp in face3Ds)
            {
                PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D, face3D_Temp, tolerance_Angle, tolerance_Distance);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                List<ISegmentable2D> segmentable2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<ISegmentable2D>();
                if (segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                    continue;

                segmentable2Ds.AddRange(segmentable2Ds_Temp);
            }

            List<Face2D> face2Ds = plane.Convert(face3D)?.Cut(segmentable2Ds, tolerance_Distance);

            return face2Ds?.ConvertAll(x => plane.Convert(x));
        }

        public static List<Shell> Cut(this Shell shell, Plane plane, double silverSpacing = Tolerance.MacroDistance, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance, double tolerance_Snap = Tolerance.MacroDistance)
        {
            if(shell == null || plane == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = shell.GetBoundingBox();
            if(boundingBox3D == null)
            {
                return null;
            }

            List<Face3D> face3Ds = shell.Face3Ds;
            if (face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            List<Face3D> face3Ds_Section = shell.Section(plane, true, tolerance_Angle, tolerance_Distance, tolerance_Snap);
            if(face3Ds_Section == null || face3Ds_Section.Count == 0)
            {
                return new List<Shell>() { new Shell(shell) };
            }

            List<Face3D> face3Ds_Cut = new List<Face3D>();
            foreach(Face3D face3D in face3Ds)
            {
                List<Face3D> face3Ds_Cut_Temp = Cut(face3D, plane, tolerance_Distance);
                if(face3Ds_Cut_Temp == null || face3Ds_Cut_Temp.Count == 0)
                {
                    face3Ds_Cut.Add(face3D);
                }
                else
                {
                    face3Ds_Cut.AddRange(face3Ds_Cut_Temp);
                }
            }

            List<Face3D> face3Ds_Above = new List<Face3D>();
            List<Face3D> face3Ds_Below = new List<Face3D>();
            foreach (Face3D face3D_Cut in face3Ds_Cut)
            {
                Point3D point3D = face3D_Cut?.GetExternalEdge3D()?.GetCentroid();
                if (point3D == null)
                {
                    continue;
                }

                if (plane.On(face3D_Cut, tolerance_Distance))
                {
                    Face2D face2D_Cut = plane.Convert(plane.Project(face3D_Cut));

                    BoundingBox2D boundingBox2D_Cut = face2D_Cut.GetBoundingBox();

                    for (int i = face3Ds_Section.Count - 1; i >= 0; i--)
                    {
                        Face2D face2D_Section = plane.Convert(face3Ds_Section[i]);
                        if (face2D_Section.GetBoundingBox().InRange(boundingBox2D_Cut, tolerance_Distance))
                        {
                            List<Face2D> face2Ds_Difference = face2D_Section.Difference(face2D_Cut, tolerance_Distance);
                            if (face2Ds_Difference != null && face2Ds_Difference.Count != 0)
                            {
                                face3Ds_Section.RemoveAt(i);
                                face3Ds_Section.AddRange(face2Ds_Difference.ConvertAll(x => plane.Convert(x)));
                            }
                            else
                            {
                                if (face2D_Section.Similar(face2D_Cut, tolerance_Distance))
                                {
                                    face3Ds_Section.RemoveAt(i);
                                }
                            }
                        }

                    }

                    Vector3D vector3D = plane.Normal * silverSpacing;

                    if(shell.Inside((Point3D)point3D.GetMoved(vector3D), silverSpacing, tolerance_Distance))
                    {
                        face3Ds_Above.Add(face3D_Cut);
                    }
                    else
                    {
                        face3Ds_Below.Add(face3D_Cut);
                    }
                }
                else
                {
                    List<Face3D> face3Ds_Temp = plane.Above(point3D) ? face3Ds_Above : face3Ds_Below;
                    face3Ds_Temp.Add(face3D_Cut);
                }
            }

            List<Shell> result = new List<Shell>();
            while(face3Ds_Section.Count > 0)
            {
                Face3D face3D_Section = face3Ds_Section[0];
                face3Ds_Section.RemoveAt(0);

                if(face3D_Section == null)
                {
                    continue;
                }

                foreach(List<Face3D> face3Ds_Temp in new List<List<Face3D>>() { face3Ds_Above, face3Ds_Below })
                {
                    List<Face3D> face3Ds_Temp_Temp = new List<Face3D>(face3Ds_Temp);
                    face3Ds_Temp_Temp.AddRange(face3Ds_Section);

                    List<Face3D> face3Ds_Shell = null;

                    face3Ds_Shell = face3D_Section.ConnectedFace3Ds(face3Ds_Temp_Temp, tolerance_Angle, tolerance_Distance);
                    if (face3Ds_Shell != null && face3Ds_Shell.Count > 1)
                    {
                        face3Ds_Shell.Add(face3D_Section);

                        Shell shell_Cut = Create.Shell(face3Ds_Shell, silverSpacing, tolerance_Distance);
                        if (shell_Cut != null)
                        {
                            face3Ds_Temp.RemoveAll(x => face3Ds_Shell.Contains(x));
                            shell_Cut.Snap(shell, tolerance_Snap, tolerance_Distance);
                            result.Add(shell_Cut);
                        }
                    }
                }
            }

            return result;
        }
    
        public static List<Face3D> Cut(this Face3D face3D, IEnumerable<Plane> planes, double tolerance = Tolerance.Distance)
        {
            if(face3D == null || planes == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>() { face3D };
            foreach (Plane plane in planes)
            {
                for (int i = result.Count - 1; i >= 0; i--)
                {
                    List<Face3D> face3Ds = result[i].Cut(plane, tolerance);
                    if(face3Ds == null || face3Ds.Count  < 2)
                    {
                        continue;
                    }

                    result.RemoveAt(i);
                    result.AddRange(face3Ds);
                }
            }

            return result;
        }
        
        public static List<Face3D> Cut(this Face3D face3D, Range<double> range, int dimensionIndex, double tolerance = Tolerance.Distance)
        {
            if (face3D == null || range == null)
            {
                return null;
            }

            Range<double> range_BoundingBox3D = face3D.GetBoundingBox()?.Range(dimensionIndex);
            if (range_BoundingBox3D == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>();

            if (System.Math.Abs(range_BoundingBox3D.Min - range_BoundingBox3D.Max) <= tolerance)
            {
                return result;
            }

            if (!range_BoundingBox3D.Intersect(range))
            {
                return result;
            }

            List<Face3D> face3Ds = face3D.Cut(new Plane[] { Create.Plane(range.Min, dimensionIndex), Create.Plane(range.Max, dimensionIndex) }, tolerance);
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return result;
            }

            foreach (Face3D face3D_Temp in face3Ds)
            {
                Range<double> range_Temp = face3D_Temp.GetBoundingBox()?.Range(dimensionIndex);
                if (range_Temp == null)
                {
                    continue;
                }

                if (!range.In(range_Temp))
                {
                    continue;
                }

                result.Add(face3D_Temp);
            }

            return result;
        }
    }
}