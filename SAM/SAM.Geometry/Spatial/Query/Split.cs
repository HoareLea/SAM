using System.Collections.Generic;

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

            List<Planar.ISegmentable2D> segmentable2Ds = new List<Planar.ISegmentable2D>();
            foreach(Face3D face3D_Shell in face3Ds)
            {
                BoundingBox3D boundingBox3D = face3D_Shell.GetBoundingBox();
                if(boundingBox3D == null)
                {
                    continue;
                }

                if(!boundingBox3D_Face3D.InRange(boundingBox3D))
                {
                    continue;
                }

                PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D, face3D_Shell, tolerance_Angle, tolerance_Distance);
                if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    continue;
                }

                List<Planar.ISegmentable2D> segmentable2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<Planar.ISegmentable2D>();
                if(segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                {
                    continue;
                }

                segmentable2Ds.AddRange(segmentable2Ds);
            }

            if(segmentable2Ds == null || segmentable2Ds.Count == 0)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();

            Planar.Face2D face2D = plane.Convert(face3D);

            List<Planar.Face2D> face2Ds = Planar.Query.Split(face2D, segmentable2Ds, tolerance_Distance);
            return face2Ds?.ConvertAll(x => plane.Convert(x));
        }
    }
}