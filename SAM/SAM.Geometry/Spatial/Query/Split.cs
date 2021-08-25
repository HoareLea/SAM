using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Split(this Face3D face3D, Shell shell, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
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

            return face3D.Split(face3Ds, tolerance_Angle, tolerance_Distance);
        }
    
        public static List<Face3D> Split(this Face3D face3D, IEnumerable<Shell> shells, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
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
                    List<Face3D> face3Ds_Temp = Split(face3D_Temp, shell, tolerance_Angle, tolerance_Distance);
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

        public static List<Face3D> Split(this Face3D face3D, IEnumerable<Face3D> face3Ds, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
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
                if (segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                {
                    continue;
                }

                segmentable2Ds.AddRange(segmentable2Ds_Temp);
            }

            if (segmentable2Ds == null || segmentable2Ds.Count == 0)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();

            Planar.Face2D face2D = plane.Convert(face3D);

            List<Planar.Face2D> face2Ds = Planar.Query.Split(face2D, segmentable2Ds, tolerance_Distance);
            return face2Ds?.ConvertAll(x => plane.Convert(x));
        }

        public static List<Face3D> Split(this Face3D face3D_ToBeSplit, Face3D face3D, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            return face3D_ToBeSplit?.Split(new Face3D[] { face3D }, tolerance_Angle, tolerance_Distance);
        }
    }
}