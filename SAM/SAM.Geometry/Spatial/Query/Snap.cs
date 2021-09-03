using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D Snap(this IEnumerable<Point3D> point3Ds, Point3D point3D, double maxDistance = double.NaN)
        {
            Point3D result = point3Ds.Closest(point3D);

            if (point3D.Distance(result) > maxDistance)
                result = new Point3D(point3D);

            return result;
        }

        public static Polygon3D Snap(this Polygon3D polygon3D, IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return null;

            List<Point3D> point3Ds_Result = polygon3D?.GetPoints();
            if (point3Ds == null || point3Ds_Result.Count == 0)
                return null;

            for (int j = 0; j < point3Ds_Result.Count; j++)
            {
                double distance = double.MaxValue;
                foreach (Point3D point3D_Temp in point3Ds)
                {
                    if (point3D_Temp == null)
                        continue;

                    double distance_Temp = point3D_Temp.Distance(point3Ds_Result[j]);
                    if (distance_Temp > 0 && distance_Temp <= tolerance && distance > distance_Temp)
                    {
                        point3Ds_Result[j] = point3D_Temp;
                        distance = distance_Temp;
                    }
                }
            }

            return new Polygon3D(point3Ds_Result);
        }

        public static Face3D Snap(this Face3D face3D_Snapped, Face3D face3D_Snapping, double snapDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            return Snap(face3D_Snapped, new Face3D[] { face3D_Snapping }, snapDistance, tolerance);
        }

        public static Shell Snap(this Shell shell_Snapped, Shell shell_Snapping, double snapDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(shell_Snapped == null || shell_Snapping == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D_Snapped = shell_Snapped.GetBoundingBox();
            if(boundingBox3D_Snapped == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D_Snapping = shell_Snapping.GetBoundingBox();
            if (boundingBox3D_Snapping == null)
            {
                return null;
            }

            if(!boundingBox3D_Snapped.InRange(boundingBox3D_Snapping, snapDistance))
            {
                return new Shell(shell_Snapped);
            }

            List<Tuple<BoundingBox3D, Face3D>> boundaries_Snapped = shell_Snapped.Boundaries;
            if (boundaries_Snapped == null || boundaries_Snapped.Count == 0)
            {
                return null;
            }

            List<Tuple<BoundingBox3D, Face3D>> boundaries_Snapping = shell_Snapping.Boundaries;
            if (boundaries_Snapping == null || boundaries_Snapping.Count == 0)
            {
                return null;
            }

            for(int i= boundaries_Snapping.Count - 1; i <=0; i--)
            {
                BoundingBox3D boundingBox3D_Boundary_Snapping = boundaries_Snapping[i].Item1;
                if (boundingBox3D_Boundary_Snapping == null || !boundingBox3D_Snapped.InRange(boundingBox3D_Boundary_Snapping, snapDistance))
                {
                    boundaries_Snapping.RemoveAt(i);
                }
            }

            List<Face3D> face3Ds_Shell = new List<Face3D>();
            for(int i=0; i < boundaries_Snapped.Count; i++)
            {
                BoundingBox3D boundingBox3D_Boundary_Snapped = boundaries_Snapped[i].Item1;
                if(boundingBox3D_Boundary_Snapped == null)
                {
                    continue;
                }

                List<Face3D> face3Ds = new List<Face3D>();
                foreach(Tuple<BoundingBox3D, Face3D> boudnary_Snapping in boundaries_Snapping)
                {
                    if(!boundingBox3D_Boundary_Snapped.InRange(boudnary_Snapping.Item1, snapDistance))
                    {
                        continue;
                    }

                    face3Ds.Add(boudnary_Snapping.Item2);
                }

                Face3D face3D_Boundary_Snapped = boundaries_Snapped[i].Item2;
                if (face3Ds == null || face3Ds.Count == 0)
                {
                    face3Ds_Shell.Add(face3D_Boundary_Snapped);
                }
                else
                {
                    face3Ds_Shell.Add(face3D_Boundary_Snapped.Snap(face3Ds, snapDistance, tolerance));
                }
            }

            return new Shell(face3Ds_Shell);
        }
        
        public static Shell Snap(this Shell shell, IEnumerable<Shell> shells_Snapping, double snapDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(shell == null || shells_Snapping == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = shell.GetBoundingBox();
            if(boundingBox3D == null)
            {
                return null;
            }

            Shell result = new Shell(shell);
            if (shells_Snapping.Count() == 0)
            {
                return result;
            }

            foreach (Shell shell_Temp in shells_Snapping)
            {
                BoundingBox3D boundingBox3D_Temp = shell_Temp?.GetBoundingBox();
                if (boundingBox3D_Temp == null)
                {
                    continue;
                }

                if (!boundingBox3D.InRange(boundingBox3D_Temp, snapDistance))
                {
                    continue;
                }

                Shell shell_Snap = result.Snap(shell_Temp, snapDistance, tolerance);
                if (shell_Snap == null)
                {
                    continue;
                }

                result = shell_Snap;
            }

            return result;
        }

        public static List<Shell> Snap(this IEnumerable<Shell> shells_Snapped, IEnumerable<Shell> shells_Snapping, double snapDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (shells_Snapped == null || shells_Snapping == null)
            {
                return null;
            }

            List<Shell> result = shells_Snapped.ToList().ConvertAll(x => x == null ? null : new Shell(x));
            if (result.Count == 0 || shells_Snapping.Count() == 0)
            {
                return result;
            }

            System.Threading.Tasks.Parallel.For(0, result.Count, (int i) =>
            {
                Shell shell = result[i];
                if(shell == null)
                {
                    return;
                }

                Shell shell_Snap = shell.Snap(shells_Snapping, snapDistance, tolerance);
                if(shell_Snap == null)
                {
                    return;
                }

                result[i] = shell_Snap;
            });

            return result;
        }

        public static Face3D Snap(this Face3D face3D, IEnumerable<Face3D> face3Ds_Snapping, double snapDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null || face3Ds_Snapping == null)
                return null;

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if(boundingBox3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            ISegmentable3D externalEdge3D = face3D.GetExternalEdge3D() as ISegmentable3D;
            List<ISegmentable3D> internalEdge3Ds = face3D.GetInternalEdge3Ds()?.ConvertAll(x => x as ISegmentable3D);

            List<Point3D> point3Ds_ExternalEdge3D = externalEdge3D.GetPoints();
            List<List<Point3D>> point3Ds_InternalEdge3Ds = internalEdge3Ds?.ConvertAll(x => x.GetPoints());

            bool snapped = false;
            foreach (Face3D face3D_Temp in face3Ds_Snapping)
            {
                BoundingBox3D boundingBox3D_Temp = face3D_Temp?.GetBoundingBox();
                if(boundingBox3D_Temp == null || !boundingBox3D.InRange(boundingBox3D_Temp, snapDistance))
                {
                    continue;
                }

                List<ISegmentable3D> segmentable3Ds = face3D_Temp.GetEdge3Ds()?.ConvertAll(x => x as ISegmentable3D);
                if(segmentable3Ds == null)
                {
                    continue;
                }
                
                
                foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                {
                    List<Segment3D> segment3Ds = segmentable3D?.GetSegments();
                    if (segment3Ds == null || segment3Ds.Count == 0)
                    {
                        continue;
                    }

                    foreach (Segment3D segment3D in segment3Ds)
                    {
                        BoundingBox3D boundingBox3D_Segment3D = segment3D.GetBoundingBox();
                        if(boundingBox3D_Segment3D == null)
                        {
                            continue;
                        }
                        
                        for(int i=0; i < point3Ds_ExternalEdge3D.Count; i++)
                        {
                            Point3D point3D = point3Ds_ExternalEdge3D[i];
                            if(!boundingBox3D_Segment3D.InRange(point3D, snapDistance))
                            {
                                continue;
                            }

                            Point3D point3D_Closest = segment3D.Closest(point3D, true);
                            double distance = point3D_Closest.Distance(point3D);
                            if(distance > snapDistance)
                            {
                                continue;
                            }

                            point3Ds_ExternalEdge3D[i] = point3D_Closest;
                            snapped = true;
                            //break;
                        }
                        
                        if(point3Ds_InternalEdge3Ds != null)
                        {
                            foreach (List<Point3D> point3Ds in point3Ds_InternalEdge3Ds)
                            {
                                for (int i = 0; i < point3Ds.Count; i++)
                                {
                                    Point3D point3D = point3Ds[i];
                                    if (!boundingBox3D_Segment3D.InRange(point3D, snapDistance))
                                    {
                                        continue;
                                    }

                                    Point3D point3D_Closest = segment3D.Closest(point3D, true);
                                    double distance = point3D_Closest.Distance(point3D);
                                    if (distance > snapDistance)
                                    {
                                        continue;
                                    }

                                    point3Ds[i] = point3D_Closest;
                                    snapped = true;
                                    //break;
                                }
                            }
                        }

                    }
                }
            }

            if (!snapped)
                return new Face3D(face3D);

            Planar.Polygon2D polygon2D_ExternalEdge2D = new Planar.Polygon2D(point3Ds_ExternalEdge3D.ConvertAll(x => plane.Convert(x)));
            List<Planar.Polygon2D> polygon2Ds_InternalEdge2Ds = point3Ds_InternalEdge3Ds?.ConvertAll(x => new Planar.Polygon2D(x.ConvertAll(y => plane.Convert(y))));

            return Face3D.Create(plane, polygon2D_ExternalEdge2D, polygon2Ds_InternalEdge2Ds);
        }

        public static Face3D Snap(this Face3D face3D, IEnumerable<Shell> shells_Snapping, double snapDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(face3D == null || shells_Snapping == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if(boundingBox3D == null)
            {
                return null;
            }

            if (shells_Snapping.Count() == 0)
            {
                return new Face3D(face3D); ;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach(Shell shell in shells_Snapping)
            {
                BoundingBox3D boundingBox3D_Shell = shell?.GetBoundingBox();
                if(boundingBox3D_Shell == null)
                {
                    continue;
                }

                if(!boundingBox3D.InRange(boundingBox3D_Shell, snapDistance))
                {
                    continue;
                }

                List<Tuple<BoundingBox3D, Face3D>> boundaries = shell.Boundaries;
                if(boundaries == null || boundaries.Count == 0)
                {
                    continue;
                }

                foreach(Tuple<BoundingBox3D, Face3D> boundary in boundaries)
                {
                    if (boundary.Item1 == null || !boundingBox3D.InRange(boundary.Item1, snapDistance))
                    {
                        continue;
                    }

                    face3Ds.Add(boundary.Item2);
                }
            }

            if(face3Ds == null || face3Ds.Count == 0)
            {
                return new Face3D(face3D); ;
            }

            return face3D.Snap(face3Ds, snapDistance, tolerance);
        }

        public static Segment3D Snap(this IEnumerable<Point3D> point3Ds, Segment3D segment3D, double maxDistance = double.NaN)
        {
            Point3D point3D_1 = Snap(point3Ds, segment3D[0], maxDistance);
            Point3D point3D_2 = Snap(point3Ds, segment3D[1], maxDistance);

            return new Segment3D(point3D_1, point3D_2);
        }

    }
}