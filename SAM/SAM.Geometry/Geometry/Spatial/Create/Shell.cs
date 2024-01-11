using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Shell Shell(this IEnumerable<Face3D> face3Ds, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (face3Ds == null || face3Ds.Count() == 0)
                return null;

            List<Face3D> face3Ds_Temp = new List<Face3D>(face3Ds);
            
            List<Tuple<Plane, List<Face3D>>> tuples = new List<Tuple<Plane, List<Face3D>>>();
            while(face3Ds_Temp.Count != 0)
            {
                Face3D face3D = face3Ds_Temp[0];
                face3Ds_Temp.RemoveAt(0);

                Plane plane = face3D.GetPlane();
                if (plane == null)
                    continue;

                List<Face3D> face3Ds_Plane = face3Ds_Temp.FindAll(x => x.GetPlane().Coplanar(plane, tolerance));
                face3Ds_Plane.RemoveAll(x => plane.Origin.Distance(x.GetPlane().Project(plane.Origin)) > tolerance);
                if(face3Ds_Plane.Count > 0)
                    face3Ds_Temp.RemoveAll(x => face3Ds_Plane.Contains(x));

                face3Ds_Plane.Add(face3D);
                tuples.Add(new Tuple<Plane, List<Face3D>>(plane, face3Ds_Plane));
            }

            List<Face3D> face3Ds_Shell = new List<Face3D>();
            foreach(Tuple<Plane, List<Face3D>> tuple in tuples)
            {
                List<Face3D> face3Ds_Plane = tuple.Item2;
                if (face3Ds_Plane == null || face3Ds_Plane.Count == 0)
                    continue;

                if(face3Ds_Plane.Count == 1)
                {
                    face3Ds_Shell.Add(face3Ds_Plane[0]);
                    continue;
                }

                List<Tuple<Face2D, Face3D>> tuples_Face2Ds = new List<Tuple<Face2D, Face3D>>();
                foreach(Face3D face3D in face3Ds_Plane)
                {
                    Face2D face2D = tuple.Item1.Convert(tuple.Item1.Project(face3D));
                    if (face2D == null)
                        continue;

                    tuples_Face2Ds.Add(new Tuple<Face2D, Face3D>(face2D, face3D));
                }

                List<Face2D> face2Ds = Planar.Query.Split(tuples_Face2Ds.ConvertAll(x => x.Item1), tolerance);
                if (tuples_Face2Ds == null || tuples_Face2Ds.Count == 0)
                    continue;

                foreach(Face2D face2D_Temp in face2Ds)
                {
                    Point2D point2D = face2D_Temp.GetInternalPoint2D();
                    if (point2D == null)
                        continue;

                    List<Tuple<Face2D, Face3D>> tuples_Face2Ds_Temp = tuples_Face2Ds.FindAll(x => x.Item1.Inside(point2D, tolerance));
                    if (tuples_Face2Ds_Temp.Count == 1)
                    {
                        Face3D face3D = tuple.Item1.Convert(face2D_Temp);
                        if (face3D != null)
                            face3Ds_Shell.Add(face3D);
                    }
                }
            }

            int count = face3Ds_Shell.Count;
            if(count < 3)
            {
                return null;
            }

            Shell result = new Shell(face3Ds_Shell);
            for(int i = face3Ds_Shell.Count - 1; i >= 0; i--)
            {
                Face3D face3D_Shell = face3Ds_Shell[i];

                Point3D point3D = face3D_Shell?.InternalPoint3D(tolerance);
                if(point3D == null)
                {
                    face3Ds_Shell.RemoveAt(i);
                    continue;
                }

                Vector3D vector3D = face3D_Shell?.GetPlane()?.Normal;
                if (vector3D == null)
                {
                    face3Ds_Shell.RemoveAt(i);
                    continue;
                }

                vector3D = vector3D * silverSpacing;

                bool inside_1 = result.Inside((Point3D)point3D.GetMoved(vector3D), silverSpacing, tolerance);
                bool inside_2 = result.Inside((Point3D)point3D.GetMoved(vector3D.GetNegated()), silverSpacing, tolerance);

                if(inside_1 == inside_2)
                {
                    face3Ds_Shell.RemoveAt(i);
                    continue;
                }
            }

            if(face3Ds_Shell.Count == count)
            {
                return result;
            }

            if(face3Ds_Shell.Count < 3)
            {
                return null;
            }

            result = new Shell(face3Ds_Shell);
            return result; 
        }

        public static Shell Shell(this Face3D face3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if(face3D == null || !face3D.IsValid() || vector3D == null || !vector3D.IsValid())
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane.Normal.Perpendicular(vector3D, tolerance))
            {
                return null;
            }

            List<ISegmentable3D> segmentable3Ds = face3D.GetEdge3Ds()?.ConvertAll(x => x as ISegmentable3D);
            if (segmentable3Ds == null || segmentable3Ds.Count == 0)
            {
                throw new NotImplementedException();
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (ISegmentable3D segmentable3D in segmentable3Ds)
            {
                List<Segment3D> segment3Ds = segmentable3D?.GetSegments();
                if (segment3Ds == null || segment3Ds.Count == 0)
                {
                    return null;
                }

                foreach (Segment3D segment3D in segment3Ds)
                {
                    Polygon3D polygon3D = Polygon3D(segment3D, vector3D, tolerance);
                    if (polygon3D != null)
                    {
                        face3Ds.Add(new Face3D(polygon3D));
                    }
                }
            }

            face3Ds.Add(face3D);
            face3Ds.Add(face3D.GetMoved(vector3D) as Face3D);

            return new Shell(face3Ds);
        }

        public static Shell Shell(this Extrusion extrusion, double tolerance = Core.Tolerance.Distance)
        {
            if(extrusion == null)
            {
                return null;
            }

            return Shell(extrusion.Face3D, extrusion.Vector, tolerance);
        }
    }
}