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
                                throw new System.NotImplementedException();
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

                    shells_Intersection.RemoveAll(x => x.Volume(silverSpacing, tolerance_Distance) < silverSpacing);
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
                            if(shells_Difference == null || shells_Difference.Count == 0)
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
    }
}